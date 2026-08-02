using ModelView_WPF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using View_WPF.Model_WPF;
using ModelView_WPF.Helpers;

namespace View_WPF.ModelView_WPF
{
    // A small class to capture data coming from the API
    public class GroupUserDto
    {
        public string UID { get; set; }
        public string DisplayName { get; set; }
    }

    public class SubjectWiseSectionAssignViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;

        // API URL updated according to Swagger and ApiSettings
        private readonly string _apiUrl;

        // Master list to hold all videos (for filtering)
        private List<ClassVideoTextModel> _allVideosMasterList = new List<ClassVideoTextModel>();

        // This property holds the received UID
        public string EntryByUid { get; set; }

        // --- Data Lists for ComboBox ---
        private ObservableCollection<ClassSubject> _subjectList;
        public ObservableCollection<ClassSubject> SubjectList
        {
            get => _subjectList;
            set { _subjectList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ClassSection> _sectionList;
        public ObservableCollection<ClassSection> SectionList
        {
            get => _sectionList;
            set { _sectionList = value; OnPropertyChanged(); }
        }

        // New: List for Group ComboBox
        private ObservableCollection<GroupUserDto> _groupList;
        public ObservableCollection<GroupUserDto> GroupList
        {
            get => _groupList;
            set { _groupList = value; OnPropertyChanged(); }
        }

        // --- Properties for ListBox (matching XAML) ---
        private ObservableCollection<dynamic> _classList;
        public ObservableCollection<dynamic> ClassList
        {
            get => _classList;
            set { _classList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<dynamic> _studentList;
        public ObservableCollection<dynamic> StudentList
        {
            get => _studentList;
            set { _studentList = value; OnPropertyChanged(); }
        }

        // --- SelectedSection property to filter videos when a section is selected ---
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
                    // Error Fix (CS1503): Instead of passing SectionId directly if it's a string,
                    // the Filter method has been adapted to accept a string.
                    SectionID = value.SectionId;
                    FilterVideosBySection(value.SectionId);
                }
            }
        }

        // --- Properties for Binding ---
        private string _uid;
        public string UID
        {
            get => _uid;
            set { _uid = value; OnPropertyChanged(); }
        }

        // Acts as Section/Class work based on database since Swagger has 'classId'
        public string SectionID { get; set; }
        public bool IsActive { get; set; }

        public ICommand SaveCommand { get; }

        // Default Constructor
        public SubjectWiseSectionAssignViewModel() : this(null) { }

        // Constructor with UID
        public SubjectWiseSectionAssignViewModel(string uid)
        {
            UID = uid;
            EntryByUid = uid;

            // Localhost self-signed SSL certificate error bypass handler setup with ApiSettings BaseUrl
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };
            _apiUrl = ApiSettings.SaveSubjectWiseAssignUrl();

            SubjectList = new ObservableCollection<ClassSubject>();
            SectionList = new ObservableCollection<ClassSection>();
            GroupList = new ObservableCollection<GroupUserDto>();
            ClassList = new ObservableCollection<dynamic>();
            StudentList = new ObservableCollection<dynamic>();

            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());

            // Initial Data Load
            _ = LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            try
            {
                // 1. Load Subject List
                var subjects = await _httpClient.GetFromJsonAsync<List<ClassSubject>>(ApiSettings.GetSubjectsUrl());
                if (subjects != null)
                {
                    SubjectList = new ObservableCollection<ClassSubject>(subjects);
                }

                // 2. Load Section List
                var sections = await _httpClient.GetFromJsonAsync<List<ClassSection>>(ApiSettings.GetSectionsUrl());
                if (sections != null)
                {
                    SectionList = new ObservableCollection<ClassSection>(sections);
                }

                // 3. New: Load Group/User List (in UID - Name format)
                var groups = await _httpClient.GetFromJsonAsync<List<GroupUserDto>>(ApiSettings.GetGroupUsersUrl());
                if (groups != null)
                {
                    GroupList = new ObservableCollection<GroupUserDto>(groups);
                }

                // 4. Load all videos and store in master list (for filtering)
                var videos = await _httpClient.GetFromJsonAsync<List<ClassVideoTextModel>>(ApiSettings.GetVideosUrl());
                if (videos != null)
                {
                    _allVideosMasterList = videos;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        // Error Fix: Changed parameter type from int to string
        private void FilterVideosBySection(string sectionId)
        {
            if (_allVideosMasterList == null) return;

            // Error Fix (CS0019): v.SectionID and sectionId are now both strings, so '==' will work properly
            var filteredVideos = _allVideosMasterList.Where(v => v.SectionID == sectionId).ToList();

            Application.Current.Dispatcher.Invoke(() =>
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
            // Validation checks
            if (string.IsNullOrEmpty(UID)) { MessageBox.Show("Please select a User (Group)!"); return; }
            if (UID?.Length > 10) { MessageBox.Show("UID max 10 chars!"); return; }
            if (string.IsNullOrEmpty(SectionID)) { MessageBox.Show("Please select a Section!"); return; }
            if (SectionID?.Length > 10) { MessageBox.Show("Section ID max 10 chars!"); return; }

            // Updated object property names according to Swagger Schema
            // The received EntryByUid property is used here as entryByUid
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
                    MessageBox.Show("Assigned Successfully!");
                }
                else
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"API Error ({response.StatusCode}):\n{errorResponse}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Error: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}