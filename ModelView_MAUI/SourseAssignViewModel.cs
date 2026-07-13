using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Storage; // FilePicker এর জন্য প্রয়োজন

namespace ModelView_MAUI
{
    public class SourseAssignViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string ApiBaseUrl = "http://74.208.227.170:8050//api/Data/";

        public SourseAssignViewModel()
        {
            BrowseCommand = new Command(async () => await ExecuteBrowse());
            SaveCommand = new Command(async () => await ExecuteUploadAndSave());
            RefreshCommand = new Command(ExecuteRefresh);
            LastUpdate = DateTime.Now;
        }

        #region Properties
        private string _courseAssignId;
        public string CourseAssignId { get => _courseAssignId; set { _courseAssignId = value; OnPropertyChanged(); } }

        private string _uid;
        public string Uid { get => _uid; set { _uid = value; OnPropertyChanged(); } }

        private string _subjectCode;
        public string SubjectCode { get => _subjectCode; set { _subjectCode = value; OnPropertyChanged(); } }

        private string _subjectName;
        public string SubjectName { get => _subjectName; set { _subjectName = value; OnPropertyChanged(); } }

        private string _entryByUid;
        public string EntryByUid { get => _entryByUid; set { _entryByUid = value; OnPropertyChanged(); } }

        private DateTime _lastUpdate;
        public DateTime LastUpdate { get => _lastUpdate; set { _lastUpdate = value; OnPropertyChanged(); } }

        private string _videoPath;
        public string VideoPath { get => _videoPath; set { _videoPath = value; OnPropertyChanged(); } }

        private bool _activate;
        public bool Activate { get => _activate; set { _activate = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        public ICommand BrowseCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand RefreshCommand { get; }
        #endregion

        #region Methods
        private async Task ExecuteBrowse()
        {
            try
            {
                var result = await FilePicker.Default.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a Video File",
                    FileTypes = FilePickerFileType.Videos
                });

                if (result != null)
                {
                    VideoPath = result.FullPath; // MAUI-তে ফাইলের পাথ
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"File picking error: {ex.Message}", "OK");
            }
        }

        private async Task ExecuteUploadAndSave()
        {
            if (string.IsNullOrEmpty(CourseAssignId) || string.IsNullOrEmpty(SubjectCode))
            {
                await Shell.Current.DisplayAlert("Validation", "Please enter Course Assign ID and Subject Code.", "OK");
                return;
            }

            try
            {
                var data = new
                {
                    CourseAssignId = this.CourseAssignId,
                    Uid = this.Uid,
                    SubjectCode = this.SubjectCode,
                    EntryByUid = this.EntryByUid,
                    VideoPath = this.VideoPath,
                    Activate = Activate ? "1" : "0",
                    LastUpdate = DateTime.Now
                };

                var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}SaveSourseAssign", data);

                if (response.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlert("Success", "Data Saved Successfully!", "OK");
                    ExecuteRefresh();
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", $"Server returned: {response.StatusCode}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
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
}