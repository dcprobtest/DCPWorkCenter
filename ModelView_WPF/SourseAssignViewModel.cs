using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json; // JSON পাঠানোর জন্য প্রয়োজন
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using DCPWorkCenter_Api.Models; // SourseAssign মডেল ব্যবহারের জন্য

namespace View_WPF.ViewModels
{
    public class SourseAssignViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private string _selectedLocalFilePath;
        // আপনার API এর লোকাল হোস্ট ইউআরএল (পোর্ট নম্বরটি আপনার Swagger অনুযায়ী চেক করে নিন)
        private const string ApiBaseUrl = "http://74.208.227.170:8050//api/Data/";

        public SourseAssignViewModel()
        {
            // Command Initialization
            BrowseCommand = new RelayCommand(ExecuteBrowse);
            SaveCommand = new RelayCommand(async () => await ExecuteUploadAndSave());
            RefreshCommand = new RelayCommand(ExecuteRefresh);

            // Default Values
            LastUpdate = DateTime.Now;
        }

        #region Properties

        private string _courseAssignId;
        public string CourseAssignId
        {
            get => _courseAssignId;
            set { _courseAssignId = value; OnPropertyChanged(); }
        }

        private string _uid;
        public string Uid
        {
            get => _uid;
            set { _uid = value; OnPropertyChanged(); }
        }

        private string _subjectCode;
        public string SubjectCode
        {
            get => _subjectCode;
            set { _subjectCode = value; OnPropertyChanged(); }
        }

        // ফোল্ডার তৈরির জন্য Subject Name প্রয়োজন
        private string _subjectName;
        public string SubjectName
        {
            get => _subjectName;
            set { _subjectName = value; OnPropertyChanged(); }
        }

        private string _entryByUid;
        public string EntryByUid
        {
            get => _entryByUid;
            set { _entryByUid = value; OnPropertyChanged(); }
        }

        private DateTime _lastUpdate;
        public DateTime LastUpdate
        {
            get => _lastUpdate;
            set { _lastUpdate = value; OnPropertyChanged(); }
        }

        private string _videoPath;
        public string VideoPath
        {
            get => _videoPath;
            set { _videoPath = value; OnPropertyChanged(); }
        }

        private bool _activate;
        public bool Activate
        {
            get => _activate;
            set { _activate = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands
        public ICommand BrowseCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand RefreshCommand { get; }
        #endregion

        #region Methods

        private void ExecuteBrowse()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Video files (*.mp4)|*.mp4|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedLocalFilePath = openFileDialog.FileName;
                VideoPath = _selectedLocalFilePath; // UI-তে পাথ দেখানোর জন্য
            }
        }

        private async Task ExecuteUploadAndSave()
        {
            // validation logic
            if (string.IsNullOrEmpty(CourseAssignId))
            {
                MessageBox.Show("Please enter Course Assign ID.");
                return;
            }

            if (string.IsNullOrEmpty(SubjectCode))
            {
                MessageBox.Show("Please enter Subject Code.");
                return;
            }

            try
            {
                // ১. ডাটাবেস মডেল অনুযায়ী ডাটা তৈরি (নতুন VideoPath এবং EntryByUid যোগ করা হয়েছে)
                var data = new SourseAssign
                {
                    CourseAssignId = this.CourseAssignId,
                    Uid = this.Uid,
                    SubjectCode = this.SubjectCode,
                    EntryByUid = this.EntryByUid, // এন্ট্রি আইডির মান সেট করা হলো
                    VideoPath = this.VideoPath,   // ভিডিও পাথের মান সেট করা হলো
                    Activate = Activate ? "1" : "0", // ডাটাবেসে string হিসেবে থাকলে
                    LastUpdate = DateTime.Now
                };

                // ২. এপিআই-তে JSON হিসেবে ডাটা পাঠানো (আপনার কন্ট্রোলারের SaveSourseAssign মেথড কল হবে)
                var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}SaveSourseAssign", data);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Data and Video Path Saved Successfully!");
                    ExecuteRefresh();
                }
                else
                {
                    MessageBox.Show($"Error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void ExecuteRefresh()
        {
            CourseAssignId = string.Empty;
            Uid = string.Empty;
            SubjectCode = string.Empty;
            SubjectName = string.Empty;
            EntryByUid = string.Empty;
            VideoPath = string.Empty;
            _selectedLocalFilePath = string.Empty;
            Activate = false;
            LastUpdate = DateTime.Now;
        }

        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    // RelayCommand Helper Class
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute();
        public event EventHandler CanExecuteChanged;
    }
}