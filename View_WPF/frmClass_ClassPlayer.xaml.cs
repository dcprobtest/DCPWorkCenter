using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModelView_WPF.Helpers;

using Point = System.Drawing.Point;

namespace View_WPF
{
    public partial class frmClass_ClassPlayer : Window, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private DispatcherTimer _screenshotTimer;
        private string _rawUID;
        private string _backendUID;
        private readonly HttpClient _httpClient;

        private int _currentSessionId = 2;
        private string _currentSectionId = null;
        private string _currentSubjectCode = null;

        private List<VideoTreeItem> _allVideosList = new List<VideoTreeItem>();
        private VideoTreeItem _currentSelectedVideo = null;

        public ObservableCollection<SubjectViewModel> CourseContent { get; set; } = new ObservableCollection<SubjectViewModel>();

        public frmClass_ClassPlayer(string uid)
        {
            InitializeComponent();
            this._rawUID = uid;
            this._backendUID = "UID_" + uid;
            this.WindowState = WindowState.Maximized;
            this.DataContext = this;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            Player.MediaFailed += (s, e) => {
                System.Windows.MessageBox.Show("Media Failed Reason: " + e.ErrorException?.Message, "Video Playback Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            Player.MediaOpened += (s, e) => {
                System.Diagnostics.Debug.WriteLine("Video opened and ready to play!");
            };

            SetupTimer();
            _ = LoadDataFromApi();

            StartScreenshotTracking();
        }

        private async Task LogActivity(string monitorTypeId, string slidePercent = "0", string slideTime = "00:00:00")
        {
            try
            {
                if (string.IsNullOrEmpty(_currentSectionId))
                {
                    return;
                }

                string eventTimeDuration = "00:00:00";
                if (Player.NaturalDuration.HasTimeSpan)
                {
                    eventTimeDuration = Player.NaturalDuration.TimeSpan.ToString(@"hh\:mm\:ss");
                }

                var payload = new
                {
                    UID = _rawUID,
                    SubjectCode = _currentSubjectCode,
                    SectionID = _currentSectionId,
                    StudyDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    EventTime = eventTimeDuration,
                    MonitorTypeID = monitorTypeId,
                    SlideBarPercent = slidePercent,
                    SlideBarTime = slideTime
                };

                var response = await _httpClient.PostAsJsonAsync(ApiSettings.ClassFocusMonitorUrl(), payload);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine("API Error: " + error);
                    System.Windows.MessageBox.Show("API Save Failed! Error: " + error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
                System.Windows.MessageBox.Show("Exception Occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadDataFromApi()
        {
            try
            {
                CourseContent.Clear();
                _allVideosList.Clear();
                string url = ApiSettings.GetAssignedVideosUrl(_rawUID);

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiSubjects = JsonSerializer.Deserialize<List<ApiSubjectResponse>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiSubjects != null && apiSubjects.Count > 0)
                    {
                        foreach (var sub in apiSubjects)
                        {
                            var subjectViewModel = new SubjectViewModel
                            {
                                SubjectCode = sub.SubjectCode,
                                SubjectDesc = sub.SubjectDesc
                            };

                            if (sub.Sections != null)
                            {
                                foreach (var sec in sub.Sections)
                                {
                                    var sectionViewModel = new SectionViewModel
                                    {
                                        SectionID = sec.SectionCode,
                                        SectionDesc = sec.SectionDesc
                                    };

                                    if (sec.Videos != null)
                                    {
                                        foreach (var v in sec.Videos)
                                        {
                                            var videoItem = new VideoTreeItem
                                            {
                                                SubjectCode = sub.SubjectCode,
                                                SectionID = v.SectionId ?? sec.SectionCode,
                                                VideoTitle = !string.IsNullOrEmpty(v.ClassContent) ? v.ClassContent : Path.GetFileName(v.ClassPath),
                                                FilePath = v.ClassPath
                                            };
                                            sectionViewModel.Videos.Add(videoItem);
                                            _allVideosList.Add(videoItem);
                                        }
                                    }
                                    subjectViewModel.Sections.Add(sectionViewModel);
                                }
                            }
                            CourseContent.Add(subjectViewModel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Problem with API connection: " + ex.Message);
            }
        }

        private void StartScreenshotTracking()
        {
            _screenshotTimer = new DispatcherTimer();
            _screenshotTimer.Interval = TimeSpan.FromSeconds(6);
            _screenshotTimer.Tick += async (s, e) => await TakeAutoCaptureAndUpload();
            _screenshotTimer.Start();
        }

        private async Task TakeAutoCaptureAndUpload()
        {
            try
            {
                Rectangle bounds = Screen.PrimaryScreen.Bounds;
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds.Size);
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                        System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                        EncoderParameters myEncoderParameters = new EncoderParameters(1);
                        EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 25L);
                        myEncoderParameters.Param[0] = myEncoderParameter;

                        bitmap.Save(ms, jpgEncoder, myEncoderParameters);
                        byte[] imageBytes = ms.ToArray();

                        using (var formData = new MultipartFormDataContent())
                        {
                            var byteContent = new ByteArrayContent(imageBytes);
                            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

                            formData.Add(byteContent, "file", $"Activity_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
                            formData.Add(new StringContent(_rawUID), "uid");
                            formData.Add(new StringContent("Employee"), "employeeName");

                            HttpResponseMessage response = await _httpClient.PostAsync(ApiSettings.UploadActivityScreenshotUrl(), formData);
                            if (!response.IsSuccessStatusCode)
                            {
                                System.Diagnostics.Debug.WriteLine("Failed to upload screenshot to server.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Capture Error: " + ex.Message);
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        private void BtnTogglePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistBorder.Visibility == Visibility.Visible)
            {
                PlaylistBorder.Visibility = Visibility.Collapsed;
                PlaylistColumn.Width = new GridLength(0);
            }
            else
            {
                PlaylistBorder.Visibility = Visibility.Visible;
                PlaylistColumn.Width = new GridLength(300);
            }
        }

        private async void TvPlaylist_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is VideoTreeItem selectedVideo)
            {
                _currentSelectedVideo = selectedVideo;
                await PlaySelectedVideo(selectedVideo);
            }
        }

        private async Task PlaySelectedVideo(VideoTreeItem video)
        {
            if (video != null && !string.IsNullOrEmpty(video.FilePath))
            {
                _currentSectionId = video.SectionID;
                _currentSubjectCode = video.SubjectCode;

                string videoUrl = video.FilePath.Trim();

                if (videoUrl.Contains("DCP_Videos", StringComparison.OrdinalIgnoreCase))
                {
                    int index = videoUrl.IndexOf("DCP_Videos", StringComparison.OrdinalIgnoreCase);
                    videoUrl = videoUrl.Substring(index + "DCP_Videos".Length);
                }

                videoUrl = videoUrl.Replace('\\', '/').TrimStart('/');

                var segments = videoUrl.Split('/');
                var encodedSegments = segments.Select(s => Uri.EscapeDataString(s));
                string relativePath = string.Join("/", encodedSegments);

                if (!videoUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    videoUrl = ApiSettings.BaseUrl.TrimEnd('/') + "/Videos/" + relativePath;
                }

                System.Diagnostics.Debug.WriteLine("Final Video URL: " + videoUrl);

                try
                {
                    Player.Source = new Uri(videoUrl);
                    Player.Play();
                    _timer.Start();
                    BtnPlay.Content = "⏸";
                    await LogActivity("4", "0", Player.Position.ToString(@"hh\:mm\:ss"));
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Video play error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) =>
            {
                if (Player.NaturalDuration.HasTimeSpan && Player.NaturalDuration.TimeSpan.TotalSeconds > 0)
                {
                    TimelineSlider.Value = Player.Position.TotalSeconds / Player.NaturalDuration.TimeSpan.TotalSeconds;
                }
            };
        }

        private async void Play_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source != null) { Player.Play(); _timer.Start(); BtnPlay.Content = "⏸"; await LogActivity("4", "0", Player.Position.ToString(@"hh\:mm\:ss")); }
        }

        private async void Pause_Click(object sender, RoutedEventArgs e) { Player.Pause(); BtnPlay.Content = "⏵"; await LogActivity("5", "0", Player.Position.ToString(@"hh\:mm\:ss")); }

        private async void Stop_Click(object sender, RoutedEventArgs e)
        {
            Player.Stop(); _timer.Stop(); BtnPlay.Content = "⏵"; TimelineSlider.Value = 0; await LogActivity("6", "0", Player.Position.ToString(@"hh\:mm\:ss"));
        }

        private async void Previous_Click(object sender, RoutedEventArgs e)
        {
            await LogActivity("3", "0", Player.Position.ToString(@"hh\:mm\:ss"));
            if (_allVideosList != null && _allVideosList.Count > 0)
            {
                int currentIndex = _currentSelectedVideo != null ? _allVideosList.IndexOf(_currentSelectedVideo) : -1;
                if (currentIndex > 0)
                {
                    _currentSelectedVideo = _allVideosList[currentIndex - 1];
                    await PlaySelectedVideo(_currentSelectedVideo);
                }
                else
                {
                    System.Windows.MessageBox.Show("This is the first video.");
                }
            }
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            await LogActivity("7", "0", Player.Position.ToString(@"hh\:mm\:ss"));
            if (_allVideosList != null && _allVideosList.Count > 0)
            {
                int currentIndex = _currentSelectedVideo != null ? _allVideosList.IndexOf(_currentSelectedVideo) : -1;
                if (currentIndex >= 0 && currentIndex < _allVideosList.Count - 1)
                {
                    _currentSelectedVideo = _allVideosList[currentIndex + 1];
                    await PlaySelectedVideo(_currentSelectedVideo);
                }
                else
                {
                    System.Windows.MessageBox.Show("This is the last video.");
                }
            }
        }

        private void TimelineSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) => _timer.Stop();

        private async void TimelineSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (Player.NaturalDuration.HasTimeSpan)
            {
                double duration = Player.NaturalDuration.TimeSpan.TotalSeconds;
                Player.Position = TimeSpan.FromSeconds(TimelineSlider.Value * duration);
            }
            _timer.Start();
            await LogActivity("2", (TimelineSlider.Value * 100).ToString("F0"), Player.Position.ToString(@"hh\:mm\:ss"));
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _ = LogActivity("8", "0", Player.Position.ToString(@"hh\:mm\:ss"));
            _timer?.Stop();
            _screenshotTimer?.Stop();
            base.OnClosing(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ApiSubjectResponse
    {
        public string SubjectCode { get; set; }
        public string SubjectDesc { get; set; }
        public List<ApiSectionResponse> Sections { get; set; }
    }

    public class ApiSectionResponse
    {
        public string SectionCode { get; set; }
        public string SectionDesc { get; set; }
        public List<ApiVideoResponse> Videos { get; set; }
    }

    public class ApiVideoResponse
    {
        public int ClassID { get; set; }
        public string SectionId { get; set; }
        public string ClassContent { get; set; }
        public string ClassPath { get; set; }
    }

    public class SubjectViewModel
    {
        public string SubjectCode { get; set; }
        public string SubjectDesc { get; set; }
        public ObservableCollection<SectionViewModel> Sections { get; set; } = new ObservableCollection<SectionViewModel>();
    }

    public class SectionViewModel
    {
        public string SectionID { get; set; }
        public string SectionDesc { get; set; }
        public ObservableCollection<VideoTreeItem> Videos { get; set; } = new ObservableCollection<VideoTreeItem>();
    }

    public class VideoTreeItem
    {
        public string SubjectCode { get; set; }
        public string SectionID { get; set; }
        public string VideoTitle { get; set; }
        public string FilePath { get; set; }
        public string Icon => "🎥 ";
    }
}