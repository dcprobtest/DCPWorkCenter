using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using System.Windows.Input;
using Model_MAUI;

namespace ModelView_MAUI
{
    public class GroupUserDto
    {
        public string UID { get; set; }
        public string DisplayName { get; set; }
    }

    public class SubjectWiseSectionAssignViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "api/Data/SaveSubjectWiseAssign";
        private List<ClassVideoTextModel> _allVideosMasterList = new List<ClassVideoTextModel>();

        public string EntryByUid { get; set; }

        private ObservableCollection<ClassSubjectModel> _subjectList;
        public ObservableCollection<ClassSubjectModel> SubjectList { get => _subjectList; set { _subjectList = value; OnPropertyChanged(); } }

        private ObservableCollection<ClassSection> _sectionList;
        public ObservableCollection<ClassSection> SectionList { get => _sectionList; set { _sectionList = value; OnPropertyChanged(); } }

        private ObservableCollection<GroupUserDto> _groupList;
        public ObservableCollection<GroupUserDto> GroupList { get => _groupList; set { _groupList = value; OnPropertyChanged(); } }

        private ObservableCollection<dynamic> _classList;
        public ObservableCollection<dynamic> ClassList { get => _classList; set { _classList = value; OnPropertyChanged(); } }

        private ObservableCollection<dynamic> _studentList;
        public ObservableCollection<dynamic> StudentList { get => _studentList; set { _studentList = value; OnPropertyChanged(); } }

        private ClassSection _selectedSection;
        public ClassSection SelectedSection
        {
            get => _selectedSection;
            set
            {
                _selectedSection = value;
                OnPropertyChanged();
                if (value != null)
                {
                    SectionID = value.SectionId;
                    FilterVideosBySection(value.SectionId);
                }
            }
        }

        private string _uid;
        public string UID { get => _uid; set { _uid = value; OnPropertyChanged(); } }

        public string SectionID { get; set; }
        public bool IsActive { get; set; }

        public ICommand SaveCommand { get; }

        public SubjectWiseSectionAssignViewModel() : this(null) { }

        public SubjectWiseSectionAssignViewModel(string uid)
        {
            UID = uid;
            EntryByUid = uid;

            // Using 10.0.2.2 for Android Emulator to access local machine
            string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "https://10.0.2.2:7073/"
                : "https://localhost:7073/";

            // Ignore SSL errors for local development
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };

            SubjectList = new ObservableCollection<ClassSubjectModel>();
            SectionList = new ObservableCollection<ClassSection>();
            GroupList = new ObservableCollection<GroupUserDto>();
            ClassList = new ObservableCollection<dynamic>();
            StudentList = new ObservableCollection<dynamic>();

            SaveCommand = new Command(async () => await ExecuteSave());

            _ = LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            try
            {
                var subjects = await _httpClient.GetFromJsonAsync<List<ClassSubjectModel>>("api/Data/GetSubjects");
                if (subjects != null) SubjectList = new ObservableCollection<ClassSubjectModel>(subjects);

                var sections = await _httpClient.GetFromJsonAsync<List<ClassSection>>("api/Data/GetSections");
                if (sections != null) SectionList = new ObservableCollection<ClassSection>(sections);

                var groups = await _httpClient.GetFromJsonAsync<List<GroupUserDto>>("api/Data/GetGroupUsers");
                if (groups != null) GroupList = new ObservableCollection<GroupUserDto>(groups);

                var videos = await _httpClient.GetFromJsonAsync<List<ClassVideoTextModel>>("api/Data/GetVideos");
                if (videos != null) _allVideosMasterList = videos;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private void FilterVideosBySection(string sectionId)
        {
            if (_allVideosMasterList == null) return;

            var filteredVideos = _allVideosMasterList.Where(v => v.SectionID == sectionId).ToList();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                ClassList.Clear();
                foreach (var video in filteredVideos)
                {
                    ClassList.Add(video);
                }
            });
        }

        private async Task ExecuteSave()
        {
            if (string.IsNullOrEmpty(UID)) { await Shell.Current.DisplayAlert("Alert", "Please select a User (Group)!", "OK"); return; }
            if (UID.Length > 10) { await Shell.Current.DisplayAlert("Alert", "UID max 10 chars!", "OK"); return; }
            if (string.IsNullOrEmpty(SectionID)) { await Shell.Current.DisplayAlert("Alert", "Please select a Section!", "OK"); return; }
            if (SectionID.Length > 10) { await Shell.Current.DisplayAlert("Alert", "Section ID max 10 chars!", "OK"); return; }

            var data = new
            {
                uid = UID,
                classId = SectionID,
                activate = IsActive ? "Y" : "N",
                entryByUid = this.EntryByUid,
                lastUpdate = DateTime.Now
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiUrl, data);

                if (response.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlert("Success", "Assigned Successfully!", "OK");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    await Shell.Current.DisplayAlert("API Error", errorResponse, "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Connection Error", ex.Message, "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}