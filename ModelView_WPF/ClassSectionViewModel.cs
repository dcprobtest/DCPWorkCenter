using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;

namespace ModelView_WPF
{
    // মডেল ক্লাস
    public class ClassSection
    {
        public string SectionId { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
        public string? SectionDesc { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? EntryByUid { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string? Activate { get; set; }
    }

    // ViewModel ক্লাস
    public class ClassSectionViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;

        private ClassSection _classSection = new ClassSection();
        public ClassSection ClassSection
        {
            get => _classSection;
            set { _classSection = value; OnPropertyChanged(); }
        }

        public ObservableCollection<object> CourseList { get; set; } = new ObservableCollection<object>();
        public ObservableCollection<ClassSection> ClassSectionList { get; set; } = new ObservableCollection<ClassSection>();

        private ClassSection _selectedClassSection;
        public ClassSection SelectedClassSection
        {
            get => _selectedClassSection;
            set
            {
                _selectedClassSection = value;
                OnPropertyChanged();

                if (_selectedClassSection != null)
                {
                    // লিস্ট থেকে সিলেক্ট করলে ডাটা ক্লোন করে বক্সে পাঠানো হচ্ছে যাতে অরিজিনাল লিস্টে ইফেক্ট না পড়ে সেভ করার আগে
                    ClassSection = new ClassSection
                    {
                        SectionId = _selectedClassSection.SectionId,
                        SubjectCode = _selectedClassSection.SubjectCode,
                        SectionDesc = _selectedClassSection.SectionDesc,
                        EntryDate = _selectedClassSection.EntryDate,
                        StartDate = _selectedClassSection.StartDate,
                        EndDate = _selectedClassSection.EndDate,
                        EntryByUid = _selectedClassSection.EntryByUid,
                        LastUpdate = _selectedClassSection.LastUpdate,
                        Activate = _selectedClassSection.Activate
                    };
                }
            }
        }

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ActiveClickCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        public ClassSectionViewModel()
        {
            // আপনার পোর্টের সাথে সামঞ্জস্য রেখে HttpClient কনফিগারেশন
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7073/") };

            AddCommand = new RelayCommand(obj => OnAdd());
            SaveCommand = new RelayCommand(async obj => await OnSave());
            ActiveClickCommand = new RelayCommand(obj => OnActive());
            DeleteCommand = new RelayCommand(async obj => await OnDelete());
            EditCommand = new RelayCommand(obj => { /* এডিট মোড লজিক */ });
            ExitCommand = new RelayCommand(obj => OnExit());

            _ = LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            try
            {
                // ১. কোর্স লিস্ট লোড
                var courses = await _httpClient.GetFromJsonAsync<List<object>>("api/Data/GetCourses");
                if (courses != null)
                {
                    CourseList.Clear();
                    foreach (var c in courses) CourseList.Add(c);
                }

                // ২. সেকশন লিস্ট লোড
                var sections = await _httpClient.GetFromJsonAsync<List<ClassSection>>("api/Data/GetSections");
                if (sections != null)
                {
                    ClassSectionList.Clear();
                    foreach (var s in sections) ClassSectionList.Add(s);
                }
            }
            catch (Exception ex)
            {
                // ডাটা লোড না হলে এরর মেসেজ দেখালে সুবিধা হবে
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private void OnAdd()
        {
            ClassSection = new ClassSection();
            ClassSection.EntryDate = DateTime.Now;
            ClassSection.StartDate = DateTime.Now;
            ClassSection.Activate = "0";
            MessageBox.Show("নতুন ক্লাস সেকশন এন্ট্রি করার জন্য তৈরি।");
        }

        private void OnActive()
        {
            if (ClassSection != null)
            {
                ClassSection.EndDate = DateTime.Now;
                ClassSection.Activate = "1";
                // যেহেতু ClassSection এর ভেতরকার প্রপার্টি পরিবর্তন হয়েছে, তাই পুরো অবজেক্টের আপডেট জানানো প্রয়োজন
                OnPropertyChanged(nameof(ClassSection));
                MessageBox.Show("সেকশনটি Active হিসেবে সেট করা হয়েছে।");
            }
        }

        private async Task OnSave()
        {
            if (string.IsNullOrEmpty(ClassSection.SectionId))
            {
                MessageBox.Show("দয়া করে Section ID প্রদান করুন।");
                return;
            }

            ClassSection.LastUpdate = DateTime.Now;
            try
            {
                // API Controller এর SaveSection মেথডকে কল করা হচ্ছে
                var response = await _httpClient.PostAsJsonAsync("api/Data/SaveSection", ClassSection);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("ডেটাবেসে সফলভাবে সেভ করা হয়েছে!");
                    await LoadInitialData(); // লিস্ট আপডেট করার জন্য পুনরায় লোড
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"সেভ করা সম্ভব হয়নি। স্ট্যাটাস কোড: {response.StatusCode}\nসার্ভার মেসেজ: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"কানেকশন এরর: {ex.Message}");
            }
        }

        private async Task OnDelete()
        {
            if (SelectedClassSection == null)
            {
                MessageBox.Show("মুছে ফেলার জন্য আগে একটি সেকশন সিলেক্ট করুন।");
                return;
            }

            var result = MessageBox.Show("আপনি কি এই সেকশনটি মুছে ফেলতে চান?", "নিশ্চিতকরণ", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // API Controller এর DeleteSection মেথডকে কল করা হচ্ছে
                    var response = await _httpClient.DeleteAsync($"api/Data/DeleteSection/{SelectedClassSection.SectionId}");
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("সফলভাবে মুছে ফেলা হয়েছে।");
                        await LoadInitialData();
                        ClassSection = new ClassSection(); // ইনপুট বক্স ক্লিয়ার করার জন্য
                    }
                    else
                    {
                        MessageBox.Show("সার্ভার থেকে মুছে ফেলা সম্ভব হয়নি।");
                    }
                }
                catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
            }
        }

        private void OnExit()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.Close();
                    break;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}