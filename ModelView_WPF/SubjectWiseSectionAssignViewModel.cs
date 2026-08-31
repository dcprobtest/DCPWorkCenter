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
    public class GroupUserDto : INotifyPropertyChanged
    {
        public string UID { get; set; }
        public string DisplayName { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class SubjectWiseSectionAssignViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private List<ClassVideoTextModel> _allVideosMasterList = new List<ClassVideoTextModel>();
        private List<ClassSection> _allSectionsMasterList = new List<ClassSection>();
        private List<GroupUserDto> _allUsersMasterList = new List<GroupUserDto>();

        public string EntryByUid { get; set; }

        private ObservableCollection<ClassSubject> _subjectList;
        public ObservableCollection<ClassSubject> SubjectList
        {
            get => _subjectList;
            set { _subjectList = value; OnPropertyChanged(); }
        }

        private ClassSubject _selectedSubject;
        public ClassSubject SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                if (_selectedSubject != value)
                {
                    _selectedSubject = value;
                    OnPropertyChanged();

                    SelectedSection = null;
                    FilterSectionsBySubject();
                }
            }
        }

        private ObservableCollection<ClassSection> _sectionList;
        public ObservableCollection<ClassSection> SectionList
        {
            get => _sectionList;
            set { _sectionList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<GroupUserDto> _groupList;
        public ObservableCollection<GroupUserDto> GroupList
        {
            get => _groupList;
            set { _groupList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<ClassVideoTextModel> _classList;
        public ObservableCollection<ClassVideoTextModel> ClassList
        {
            get => _classList;
            set { _classList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<GroupUserDto> _studentList;
        public ObservableCollection<GroupUserDto> StudentList
        {
            get => _studentList;
            set { _studentList = value; OnPropertyChanged(); }
        }

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
        public string UID
        {
            get => _uid;
            set { _uid = value; OnPropertyChanged(); }
        }

        public string SectionID { get; set; }
        public bool IsActive { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand LoadUsersCommand { get; }
        public ICommand ExitCommand { get; }

        public SubjectWiseSectionAssignViewModel() : this(null) { }

        public SubjectWiseSectionAssignViewModel(string uid)
        {
            UID = uid;
            EntryByUid = uid;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };
            _apiUrl = ApiSettings.SaveSubjectWiseAssignUrl();

            SubjectList = new ObservableCollection<ClassSubject>();
            SectionList = new ObservableCollection<ClassSection>();
            GroupList = new ObservableCollection<GroupUserDto>();
            ClassList = new ObservableCollection<ClassVideoTextModel>();
            StudentList = new ObservableCollection<GroupUserDto>();

            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
            LoadUsersCommand = new RelayCommand((obj) => PopulateUserList());
            ExitCommand = new RelayCommand((obj) => ExecuteExit(obj));

            _ = LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            try
            {
                var subjects = await _httpClient.GetFromJsonAsync<List<ClassSubject>>(ApiSettings.GetSubjectsUrl());
                if (subjects != null)
                {
                    SubjectList = new ObservableCollection<ClassSubject>(subjects);
                }

                var sections = await _httpClient.GetFromJsonAsync<List<ClassSection>>(ApiSettings.GetSectionsUrl());
                if (sections != null)
                {
                    _allSectionsMasterList = sections;
                }

                var groups = await _httpClient.GetFromJsonAsync<List<GroupUserDto>>(ApiSettings.GetGroupUsersUrl());
                if (groups != null)
                {
                    GroupList = new ObservableCollection<GroupUserDto>(groups);
                    _allUsersMasterList = groups;
                }

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

        private void FilterSectionsBySubject()
        {
            SectionList.Clear();
            ClassList.Clear();

            if (SelectedSubject == null || _allSectionsMasterList == null) return;

            var selectedCode = SelectedSubject.SubjectCode?.Trim();

            var filteredSections = _allSectionsMasterList
                .Where(s => !string.IsNullOrEmpty(s.SubjectCode) &&
                            s.SubjectCode.Trim() == selectedCode)
                .ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var section in filteredSections)
                {
                    SectionList.Add(section);
                }
            });
        }

        private void PopulateUserList()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StudentList.Clear();
                foreach (var user in _allUsersMasterList)
                {
                    StudentList.Add(user);
                }
            });
        }

        private void FilterVideosBySection(string sectionId)
        {
            if (_allVideosMasterList == null) return;

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
            var selectedUsers = StudentList.Where(u => u.IsSelected).ToList();
            var selectedVideos = ClassList.Where(v => v.IsSelected).ToList();

            if (!selectedUsers.Any())
            {
                MessageBox.Show("Please select at least one User from the list!");
                return;
            }

            if (!selectedVideos.Any())
            {
                MessageBox.Show("Please select at least one Video from the list!");
                return;
            }

            if (string.IsNullOrEmpty(SectionID)) { MessageBox.Show("Please select a Section!"); return; }
            if (SectionID?.Length > 10) { MessageBox.Show("Section ID max 10 chars!"); return; }

            foreach (var user in selectedUsers)
            {
                foreach (var video in selectedVideos)
                {
                    var data = new
                    {
                        uid = user.UID,
                        classId = SectionID,
                        videoId = video.ClassID,
                        activate = "Y",
                        entryByUid = this.EntryByUid,
                        lastUpdate = DateTime.Now
                    };

                    try
                    {
                        var response = await _httpClient.PostAsJsonAsync(_apiUrl, data);
                        if (!response.IsSuccessStatusCode)
                        {
                            var errorResponse = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"API Error for {user.DisplayName} ({response.StatusCode}):\n{errorResponse}");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Connection Error: " + ex.Message);
                        return;
                    }
                }
            }

            MessageBox.Show("Assigned Successfully!");
        }

        private void ExecuteExit(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
            else
            {
                foreach (Window win in Application.Current.Windows)
                {
                    if (win.DataContext == this)
                    {
                        win.Close();
                        break;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}