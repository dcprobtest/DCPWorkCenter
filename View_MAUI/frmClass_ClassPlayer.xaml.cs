using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Views;

namespace View_MAUI
{
    public partial class frmClass_ClassPlayer : ContentPage, INotifyPropertyChanged
    {
        private IDispatcherTimer _timer;
        private IDispatcherTimer _screenshotTimer;
        private string _rawUID;
        private string _backendUID;

        private string _screenshotRootPath = @"C:\DCP_UserActivity";
        private readonly HttpClient _httpClient = new HttpClient();

        public ObservableCollection<VideoTreeItem> CourseContent { get; set; } = new ObservableCollection<VideoTreeItem>();

        public frmClass_ClassPlayer(string uid)
        {
            InitializeComponent();
            this._rawUID = uid;
            this._backendUID = "UID_" + uid;
            this.BindingContext = this;

            if (DeviceInfo.Current.Platform != DevicePlatform.WinUI)
            {
                _screenshotRootPath = Path.Combine(FileSystem.AppDataDirectory, "DCP_UserActivity");
            }

            SetupTimer();
            _ = LoadDataFromApi();

            CleanupOldScreenshots();
            StartScreenshotTracking();
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
                        foreach (var v in apiVideos)
                        {
                            CourseContent.Add(new VideoTreeItem
                            {
                                VideoTitle = v.ClassContent,
                                FilePath = v.ClassPath
                            });
                        }

                        CvPlaylist.ItemsSource = CourseContent;
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Data not found from API. Status Code: " + response.StatusCode, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Problem", "Problem with API connection: " + ex.Message, "OK");
            }
        }

        private void StartScreenshotTracking()
        {
            _screenshotTimer = Dispatcher.CreateTimer();
            _screenshotTimer.Interval = TimeSpan.FromSeconds(6);
            _screenshotTimer.Tick += (s, e) => TakeAutoCapture();
            _screenshotTimer.Start();
        }

        private async void TakeAutoCapture()
        {
            try
            {
                string userFolderPath = Path.Combine(_screenshotRootPath, _backendUID);
                if (!Directory.Exists(userFolderPath)) Directory.CreateDirectory(userFolderPath);

                string fileName = Path.Combine(userFolderPath, $"Activity_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");

                if (Screenshot.Default.IsCaptureSupported)
                {
                    IScreenshotResult screenshot = await Screenshot.Default.CaptureAsync();
                    using (Stream stream = await screenshot.OpenReadAsync(ScreenshotFormat.Jpeg))
                    using (FileStream fileStream = File.Create(fileName))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
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

        private void BtnTogglePlaylist_Clicked(object sender, EventArgs e)
        {
            if (PlaylistBorder.IsVisible)
            {
                PlaylistBorder.IsVisible = false;
                PlaylistColumn.Width = new GridLength(0);
            }
            else
            {
                PlaylistBorder.IsVisible = true;
                PlaylistColumn.Width = new GridLength(300);
            }
        }

        private void CvPlaylist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is VideoTreeItem selectedVideo)
            {
                if (!string.IsNullOrEmpty(selectedVideo.FilePath) && File.Exists(selectedVideo.FilePath))
                {
                    Player.Source = MediaSource.FromFile(selectedVideo.FilePath);
                    Player.Play();
                    _timer.Start();
                    BtnPlay.Text = "⏸";
                }
                else
                {
                    DisplayAlert("Not Found", "Video file not found. Check path: " + selectedVideo.FilePath, "OK");
                }
            }
        }

        private void SetupTimer()
        {
            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                if (Player.Duration != TimeSpan.Zero)
                {
                    TimelineSlider.Value = Player.Position.TotalSeconds / Player.Duration.TotalSeconds;
                }
            };
        }

        private void Play_Clicked(object sender, EventArgs e)
        {
            if (Player.Source != null)
            {
                Player.Play();
                _timer.Start();
                BtnPlay.Text = "⏸";
            }
        }

        private void Pause_Clicked(object sender, EventArgs e)
        {
            Player.Pause();
            BtnPlay.Text = "⏵";
        }

        private void Stop_Clicked(object sender, EventArgs e)
        {
            Player.Stop();
            _timer.Stop();
            BtnPlay.Text = "⏵";
            TimelineSlider.Value = 0;
        }

        private async void Previous_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Info", "The Previous Video feature has not been implemented yet.", "OK");
        }

        private async void Next_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Info", "The Next Video feature has not been implemented yet.", "OK");
        }

        private void TimelineSlider_DragStarted(object sender, EventArgs e) => _timer.Stop();

        private void TimelineSlider_DragCompleted(object sender, EventArgs e)
        {
            if (Player.Duration != TimeSpan.Zero)
            {
                double duration = Player.Duration.TotalSeconds;
                Player.SeekTo(TimeSpan.FromSeconds(TimelineSlider.Value * duration));
            }
            _timer.Start();
        }

        protected override void OnDisappearing()
        {
            _timer?.Stop();
            _screenshotTimer?.Stop();
            Player?.Handler?.DisconnectHandler();
            base.OnDisappearing();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ApiVideoResponse
    {
        public string ClassContent { get; set; }
        public string ClassPath { get; set; }
    }

    public class VideoTreeItem
    {
        public string VideoTitle { get; set; }
        public string FilePath { get; set; }
        public string Icon => "🎥 ";
    }
}