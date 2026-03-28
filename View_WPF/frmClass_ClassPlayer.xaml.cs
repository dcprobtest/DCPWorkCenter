using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Drawing; // Add Reference: System.Drawing
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms; // Add Reference: System.Windows.Forms

// Point এর কনফ্লিক্ট দূর করার জন্য নির্দিষ্ট করে দেওয়া হলো
using Point = System.Drawing.Point;

namespace View_WPF
{
    public partial class frmClass_ClassPlayer : Window, INotifyPropertyChanged
    {
        private DispatcherTimer _timer;
        private DispatcherTimer _screenshotTimer; // স্ক্রিনশটের জন্য আলাদা টাইমার
        private string _backendUID; // ব্যাকএন্ডে UID ধরে রাখার জন্য
        private string _screenshotRootPath = @"C:\DCP_UserActivity";

        // আপনার SQL Server কানেকশন স্ট্রিং
        private string connectionString = @"Server=DESKTOP-NDHKDBI\SQLEXPRESS;Database=DCPWorkCenter;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        // TreeView-এর জন্য মূল ডাটা সোর্স
        public ObservableCollection<SubjectViewModel> CourseContent { get; set; } = new ObservableCollection<SubjectViewModel>();

        // আপডেট করা কনস্ট্রাক্টর যা UID রিসিভ করবে
        public frmClass_ClassPlayer(string uid)
        {
            InitializeComponent();

            // আইডি ব্যাকএন্ডে সেভ করা হলো
            this._backendUID = "UID_" + uid;
            this.WindowState = WindowState.Maximized;

            // DataContext সেট করা হয়েছে যাতে XAML-এ {Binding CourseContent} কাজ করে
            this.DataContext = this;

            SetupTimer();
            LoadTreeViewData();

            // স্ক্রিনশট লজিক শুরু
            CleanupOldScreenshots(); // পুরনো ফাইল ডিলিট
            StartScreenshotTracking(); // টাইমার শুরু
        }

        // --- স্ক্রিনশট লজিক শুরু ---

        private void StartScreenshotTracking()
        {
            _screenshotTimer = new DispatcherTimer();
            _screenshotTimer.Interval = TimeSpan.FromSeconds(6); // প্রতি ৬ সেকেন্ড অন্তর
            _screenshotTimer.Tick += (s, e) => TakeAutoCapture();
            _screenshotTimer.Start();
        }

        private void TakeAutoCapture()
        {
            try
            {
                // ইউজার আইডি অনুযায়ী ফোল্ডার পাথ
                string userFolderPath = Path.Combine(_screenshotRootPath, _backendUID);
                if (!Directory.Exists(userFolderPath)) Directory.CreateDirectory(userFolderPath);

                string fileName = Path.Combine(userFolderPath, $"Activity_{DateTime.Now:yyyyMMdd_HHmmss}.jpg");

                // স্ক্রিন ক্যাপচার
                Rectangle bounds = Screen.PrimaryScreen.Bounds;
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        // সরাসরি System.Drawing.Point ব্যবহার করা হয়েছে
                        g.CopyFromScreen(System.Drawing.Point.Empty, System.Drawing.Point.Empty, bounds.Size);
                    }

                    // সাইজ কমানোর জন্য কোয়ালিটি ২৫% করা হলো (৬০ কেবির নিচে থাকবে)
                    ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                    EncoderParameters myEncoderParameters = new EncoderParameters(1);
                    EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 25L);
                    myEncoderParameters.Param[0] = myEncoderParameter;

                    bitmap.Save(fileName, jpgEncoder, myEncoderParameters);
                }
            }
            catch { /* ব্যাকএন্ড এরর ইগনোর করা হলো */ }
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

        // --- স্ক্রিনশট লজিক শেষ ---

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

        private void LoadTreeViewData()
        {
            try
            {
                CourseContent.Clear();
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string sqlSubject = "SELECT SubjectCode, SubjectDesc FROM ClassSubject WHERE Activate = '1'";
                    using (SqlCommand cmdSub = new SqlCommand(sqlSubject, con))
                    using (SqlDataReader drSub = cmdSub.ExecuteReader())
                    {
                        while (drSub.Read())
                        {
                            var subject = new SubjectViewModel
                            {
                                SubjectCode = drSub["SubjectCode"].ToString(),
                                SubjectDesc = drSub["SubjectDesc"].ToString()
                            };
                            LoadSectionsForSubject(subject);
                            CourseContent.Add(subject);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("ডাটা লোড করতে সমস্যা: " + ex.Message);
            }
        }

        private void LoadSectionsForSubject(SubjectViewModel subject)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string sqlSection = "SELECT SectionID, SectionDesc FROM ClassSection WHERE SubjectCode = @SubCode AND Activate = '1'";
                using (SqlCommand cmdSec = new SqlCommand(sqlSection, con))
                {
                    cmdSec.Parameters.AddWithValue("@SubCode", subject.SubjectCode);
                    using (SqlDataReader drSec = cmdSec.ExecuteReader())
                    {
                        while (drSec.Read())
                        {
                            var section = new SectionViewModel
                            {
                                SectionID = drSec["SectionID"].ToString(),
                                SectionDesc = drSec["SectionDesc"].ToString()
                            };
                            LoadVideosForSection(section);
                            subject.Sections.Add(section);
                        }
                    }
                }
            }
        }

        private void LoadVideosForSection(SectionViewModel section)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                string sqlVideo = "SELECT ClassContent, ClassPath FROM ClassVideoText WHERE SectionID = @SecID AND Activate = '1'";
                using (SqlCommand cmdVid = new SqlCommand(sqlVideo, con))
                {
                    cmdVid.Parameters.AddWithValue("@SecID", section.SectionID);
                    using (SqlDataReader drVid = cmdVid.ExecuteReader())
                    {
                        while (drVid.Read())
                        {
                            section.Videos.Add(new VideoTreeItem
                            {
                                VideoTitle = drVid["ClassContent"].ToString(),
                                FilePath = drVid["ClassPath"].ToString()
                            });
                        }
                    }
                }
            }
        }

        private void TvPlaylist_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is VideoTreeItem selectedVideo)
            {
                if (!string.IsNullOrEmpty(selectedVideo.FilePath) && File.Exists(selectedVideo.FilePath))
                {
                    Player.Source = new Uri(selectedVideo.FilePath);
                    Player.Play();
                    _timer.Start();
                    BtnPlay.Content = "⏸";
                }
                else
                {
                    System.Windows.MessageBox.Show("ভিডিও ফাইলটি খুঁজে পাওয়া যায়নি: " + selectedVideo.FilePath);
                }
            }
        }

        private void SetupTimer()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) =>
            {
                if (Player.NaturalDuration.HasTimeSpan)
                {
                    if (Player.NaturalDuration.TimeSpan.TotalSeconds > 0)
                        TimelineSlider.Value = Player.Position.TotalSeconds / Player.NaturalDuration.TimeSpan.TotalSeconds;
                }
            };
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Source != null) { Player.Play(); _timer.Start(); BtnPlay.Content = "⏸"; }
        }

        private void Pause_Click(object sender, RoutedEventArgs e) { Player.Pause(); BtnPlay.Content = "⏵"; }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            Player.Stop(); _timer.Stop(); BtnPlay.Content = "⏵"; TimelineSlider.Value = 0;
        }

        private void Next_Click(object sender, RoutedEventArgs e) { }
        private void Previous_Click(object sender, RoutedEventArgs e) { }

        private void TimelineSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) => _timer.Stop();

        private void TimelineSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (Player.NaturalDuration.HasTimeSpan)
            {
                double duration = Player.NaturalDuration.TimeSpan.TotalSeconds;
                Player.Position = TimeSpan.FromSeconds(TimelineSlider.Value * duration);
            }
            _timer.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _timer?.Stop();
            _screenshotTimer?.Stop(); // বন্ধ করার সময় স্ক্রিনশট টাইমার স্টপ
            base.OnClosing(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // --- ডাটা মডেলসমূহ ---

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
        public string VideoTitle { get; set; }
        public string FilePath { get; set; }
    }
}