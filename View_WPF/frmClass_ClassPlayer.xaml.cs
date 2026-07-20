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

using Point = System.Drawing.Point;

namespace View_WPF
{
    public partial class frmClass_ClassPlayer : Window, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private DispatcherTimer _screenshotTimer;
        private string _rawUID;
        private string _backendUID;
        private string _screenshotRootPath = @"C:\DCP_UserActivity";
        private readonly HttpClient _httpClient = new HttpClient();

        private int _currentSessionId = 2;
        private string _currentSectionId = null;

        public ObservableCollection<SubjectViewModel> CourseContent { get; set; } = new ObservableCollection<SubjectViewModel>();

        public frmClass_ClassPlayer(string uid)
        {
            InitializeComponent();
            this._rawUID = uid;
            this._backendUID = "UID_" + uid;
            this.WindowState = WindowState.Maximized;
            this.DataContext = this;

            SetupTimer();
            _ = LoadDataFromApi();

            CleanupOldScreenshots();
            StartScreenshotTracking();
        }

        private async Task LogActivity(string monitorTypeId, string slidePercent = "0", string slideTime = "00:00:00")
        {
            try
            {
                var payload = new
                {
                    SessionID = _currentSessionId,
                    SectionID = _currentSectionId,
                    EventTime = DateTime.Now,
                    MonitorTypeID = monitorTypeId,
                    SlideBarPercent = slidePercent,
                    SlideBarTime = slideTime
                };

                string apiUrl = "https://localhost:7073/api/ClassFocusMonitor";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, payload);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine("API Error: " + error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
            }
        }

        private async Task LoadDataFromApi()
        {
            try
            {
                CourseContent.Clear();
                string url = $"https://localhost:7073/api/Data/GetAssignedVideos/{_rawUID}";

                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiVideos = JsonSerializer.Deserialize<List<ApiVideoResponse>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiVideos != null && apiVideos.Count > 0)
                    {
                        var subject = new SubjectViewModel { SubjectDesc = "My Enrolled Courses" };
                        var section = new SectionViewModel { SectionDesc = "Assigned Lessons" };

                        foreach (var v in apiVideos)
                        {
                            section.Videos.Add(new VideoTreeItem
                            {
                                SectionID = v.SectionID,
                                VideoTitle = v.ClassContent,
                                FilePath = v.ClassPath
                            });
                        }

                        subject.Sections.Add(section);
                        CourseContent.Add(subject);
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
            _screenshotTimer.Tick += (s, e) => TakeAutoCapture();
            _screenshotTimer.Start();
        }

        private void TakeAutoCapture()
        {
            try
            {
                string userFolderPath = Path.Combine(_screenshotRootPath, _backendUID);
                if (!Directory.Exists(userFolderPath)) Directory.CreateDirectory(userFolderPath);
                string fileName = Path.Combine(userFolderPath, $"Activity_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");
                Rectangle bounds = Screen.PrimaryScreen.Bounds;
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds.Size);
                    }
                    ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                    EncoderParameters myEncoderParameters = new EncoderParameters(1);
                    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 25L);
                    myEncoderParameters.Param[0] = myEncoderParameter;
                    bitmap.Save(fileName, jpgEncoder, myEncoderParameters);
                }
            }
            catch { }
        }

        private void CleanupOldScreenshots()
        {
            try
            {
                if (!Directory.Exists(_screenshotRootPath)) return;
                var threshold = DateTime.Now.AddDays(-4);
                var files = Directory.GetFiles(_screenshotRootPath, "*.jpg", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.CreationTime < threshold) fi.Delete();
                }
            }
            catch { }
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
                if (!string.IsNullOrEmpty(selectedVideo.FilePath) && File.Exists(selectedVideo.FilePath))
                {
                    _currentSectionId = selectedVideo.SectionID;

                    Player.Source = new Uri(selectedVideo.FilePath);
                    Player.Play();
                    _timer.Start();
                    BtnPlay.Content = "⏸";
                    await LogActivity("4");
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
            if (Player.Source != null) { Player.Play(); _timer.Start(); BtnPlay.Content = "⏸"; await LogActivity("4"); }
        }

        private async void Pause_Click(object sender, RoutedEventArgs e) { Player.Pause(); BtnPlay.Content = "⏵"; await LogActivity("5"); }

        private async void Stop_Click(object sender, RoutedEventArgs e)
        {
            Player.Stop(); _timer.Stop(); BtnPlay.Content = "⏵"; TimelineSlider.Value = 0; await LogActivity("6");
        }

        private async void Previous_Click(object sender, RoutedEventArgs e)
        {
            await LogActivity("3");
            System.Windows.MessageBox.Show("The Previous Video feature has not been implemented yet.");
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            await LogActivity("7");
            System.Windows.MessageBox.Show("The Next Video feature has not been implemented yet.");
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
            _ = LogActivity("8");
            _timer?.Stop();
            _screenshotTimer?.Stop();
            base.OnClosing(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ApiVideoResponse
    {
        public string SectionID { get; set; }
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
        public string SectionID { get; set; }
        public string VideoTitle { get; set; }
        public string FilePath { get; set; }
        public string Icon => "🎥 ";
    }
}