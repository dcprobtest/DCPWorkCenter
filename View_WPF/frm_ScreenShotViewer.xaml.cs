using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ModelView_WPF.Helpers;

namespace View_WPF
{
    public partial class frm_ScreenShotViewer : Window
    {
        private readonly HttpClient _httpClient;
        private DispatcherTimer _slideshowTimer;
        private List<string> _screenshotFiles = new List<string>();
        private int _currentIndex = 0;
        private string _selectedUID = "";

        public frm_ScreenShotViewer()
        {
            InitializeComponent();

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            SetupSlideshowTimer();
            _ = LoadUIDsToComboBox();
        }

        private void SetupSlideshowTimer()
        {
            _slideshowTimer = new DispatcherTimer();
            _slideshowTimer.Interval = TimeSpan.FromSeconds(1);
            _slideshowTimer.Tick += (s, e) => ShowNextSlide();
        }

        private async Task LoadUIDsToComboBox()
        {
            try
            {
                var response = await _httpClient.GetAsync(ApiSettings.GetActivityUIDsUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var uids = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    CmbUIDs.ItemsSource = uids;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading UIDs: " + ex.Message);
            }
        }

        private async void CmbUIDs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUIDs.SelectedItem != null)
            {
                _selectedUID = CmbUIDs.SelectedItem.ToString();
                await LoadScreenshotsForDate();
            }
        }

        private async void DpActivityDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            await LoadScreenshotsForDate();
        }

        private async Task LoadScreenshotsForDate()
        {
            if (string.IsNullOrEmpty(_selectedUID) || DpActivityDate.SelectedDate == null) return;

            string dateStr = DpActivityDate.SelectedDate.Value.ToString("yyyyMMdd");
            try
            {
                string url = ApiSettings.GetScreenshotsUrl(_selectedUID, dateStr);
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    _screenshotFiles = JsonSerializer.Deserialize<List<string>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    _currentIndex = 0;

                    if (_screenshotFiles != null && _screenshotFiles.Count > 0)
                    {
                        EmptyPlaceholder.Visibility = Visibility.Collapsed;
                        TimelineSlider.Maximum = _screenshotFiles.Count - 1;
                        TimelineSlider.Value = 0;
                        TxtTotalTime.Text = TimeSpan.FromSeconds(_screenshotFiles.Count).ToString(@"hh\:mm\:ss");
                    }
                    else
                    {
                        EmptyPlaceholder.Visibility = Visibility.Visible;
                        ImgPlayer.Source = null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading screenshots: " + ex.Message);
            }
        }

        private void BtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (_screenshotFiles != null && _screenshotFiles.Count > 0)
            {
                _slideshowTimer.Start();
            }
            else
            {
                System.Windows.MessageBox.Show("No screenshots available to play for the selected date.");
            }
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            _slideshowTimer.Stop();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            _slideshowTimer.Stop();
            _currentIndex = 0;
            TimelineSlider.Value = 0;
            ImgPlayer.Source = null;
            EmptyPlaceholder.Visibility = Visibility.Visible;
            TxtCurrentTime.Text = "00:00:00";
        }

        private async void ShowNextSlide()
        {
            if (_screenshotFiles == null || _screenshotFiles.Count == 0) return;

            if (_currentIndex >= _screenshotFiles.Count)
            {
                _currentIndex = 0;
            }

            string fileName = _screenshotFiles[_currentIndex];
            string imageUrl = ApiSettings.GetScreenshotImageUrl(_selectedUID, fileName);

            try
            {
                var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                using (var ms = new MemoryStream(imageBytes))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    ImgPlayer.Source = bitmap;
                    EmptyPlaceholder.Visibility = Visibility.Collapsed;
                }

                TimelineSlider.Value = _currentIndex;
                TxtCurrentTime.Text = TimeSpan.FromSeconds(_currentIndex).ToString(@"hh\:mm\:ss");
            }
            catch { }

            _currentIndex++;
        }

        private async void TimelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_screenshotFiles == null || _screenshotFiles.Count == 0) return;

            int targetIndex = (int)Math.Round(TimelineSlider.Value);
            if (targetIndex >= 0 && targetIndex < _screenshotFiles.Count && targetIndex != _currentIndex)
            {
                _currentIndex = targetIndex;
                string fileName = _screenshotFiles[_currentIndex];
                string imageUrl = ApiSettings.GetScreenshotImageUrl(_selectedUID, fileName);

                try
                {
                    var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                    using (var ms = new MemoryStream(imageBytes))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        ImgPlayer.Source = bitmap;
                        EmptyPlaceholder.Visibility = Visibility.Collapsed;
                    }

                    TxtCurrentTime.Text = TimeSpan.FromSeconds(_currentIndex).ToString(@"hh\:mm\:ss");
                }
                catch { }
            }
        }
    }
}