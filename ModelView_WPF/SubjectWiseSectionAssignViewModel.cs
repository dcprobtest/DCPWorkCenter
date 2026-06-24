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
using View_WPF.Model_WPF; // ClassVideoTextModel এর জন্য এটি প্রয়োজন

namespace View_WPF.ModelView_WPF
{
    // API থেকে আসা ডেটা ধরার জন্য একটি ছোট ক্লাস
    public class GroupUserDto
    {
        public string UID { get; set; }
        public string DisplayName { get; set; }
    }

    public class SubjectWiseSectionAssignViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;

        // Swagger অনুযায়ী সঠিক API URL আপডেট করা হয়েছে
        private readonly string _apiUrl = "api/Data/SaveSubjectWiseAssign";

        // সব ভিডিওর মাস্টার লিস্ট রাখার জন্য (ফিল্টারিংয়ের জন্য)
        private List<ClassVideoTextModel> _allVideosMasterList = new List<ClassVideoTextModel>();

        // এই প্রোপার্টিটি রিসিভ করা UID ধরে রাখবে
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

        // নতুন: Group ComboBox-এর জন্য লিস্ট
        private ObservableCollection<GroupUserDto> _groupList;
        public ObservableCollection<GroupUserDto> GroupList
        {
            get => _groupList;
            set { _groupList = value; OnPropertyChanged(); }
        }

        // --- ListBox-এর জন্য প্রোপার্টি (XAML এর সাথে মিল রেখে) ---
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

        // --- সেকশন সিলেক্ট করলে ভিডিও ফিল্টার করার জন্য SelectedSection প্রোপার্টি ---
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
                    // Error Fix (CS1503): SectionId string হলে সরাসরি পাস না করে 
                    // Filter মেথডটি string গ্রহণ করার উপযোগী করা হয়েছে।
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

        // Swagger-এ 'classId' নাম থাকায় ডাটাবেস অনুযায়ী এটিই Section/Class এর কাজ করবে
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
            // আপনার পোর্টের সাথে সামঞ্জস্য রেখে HttpClient (বাকি কোডের মতো)
            _httpClient = new HttpClient { BaseAddress = new Uri("http://74.208.227.170:8050//") };

            SubjectList = new ObservableCollection<ClassSubject>();
            SectionList = new ObservableCollection<ClassSection>();
            GroupList = new ObservableCollection<GroupUserDto>();
            ClassList = new ObservableCollection<dynamic>(); // Initialize
            StudentList = new ObservableCollection<dynamic>(); // Initialize

            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());

            // Initial Data Load
            _ = LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            try
            {
                // ১. সাবজেক্ট লিস্ট লোড
                var subjects = await _httpClient.GetFromJsonAsync<List<ClassSubject>>("api/Data/GetSubjects");
                if (subjects != null)
                {
                    SubjectList = new ObservableCollection<ClassSubject>(subjects);
                }

                // ২. সেকশন লিস্ট লোড
                var sections = await _httpClient.GetFromJsonAsync<List<ClassSection>>("api/Data/GetSections");
                if (sections != null)
                {
                    SectionList = new ObservableCollection<ClassSection>(sections);
                }

                // ৩. নতুন: গ্রুপ/ইউজার লিস্ট লোড (UID - Name ফরম্যাটে)
                var groups = await _httpClient.GetFromJsonAsync<List<GroupUserDto>>("api/Data/GetGroupUsers");
                if (groups != null)
                {
                    GroupList = new ObservableCollection<GroupUserDto>(groups);
                }

                // ৪. সব ভিডিও লোড করে মাস্টার লিস্টে রাখা (ফিল্টারিংয়ের জন্য)
                var videos = await _httpClient.GetFromJsonAsync<List<ClassVideoTextModel>>("api/Data/GetVideos");
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

        // Error Fix: প্যারামিটার int থেকে string এ পরিবর্তন করা হয়েছে
        private void FilterVideosBySection(string sectionId)
        {
            if (_allVideosMasterList == null) return;

            // Error Fix (CS0019): v.SectionID এবং sectionId এখন উভয়ই string, তাই '==' কাজ করবে
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
            // ভ্যালিডেশন চেক
            if (string.IsNullOrEmpty(UID)) { MessageBox.Show("Please select a User (Group)!"); return; }
            if (UID?.Length > 10) { MessageBox.Show("UID max 10 chars!"); return; }
            if (string.IsNullOrEmpty(SectionID)) { MessageBox.Show("Please select a Section!"); return; }
            if (SectionID?.Length > 10) { MessageBox.Show("Section ID max 10 chars!"); return; }

            // Swagger Schema অনুযায়ী অবজেক্ট প্রোপার্টি নেমগুলো আপডেট করা হয়েছে
            // এখানে entryByUid হিসেবে রিসিভ করা EntryByUid প্রোপার্টিটি ব্যবহার করা হয়েছে
            var data = new
            {
                uid = UID,
                classId = SectionID,
                activate = IsActive ? "Y" : "N",
                entryByUid = this.EntryByUid, // এখানে রিসিভ করা UID টি বসবে
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