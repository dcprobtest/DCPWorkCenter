using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls; // MAUI কমান্ড এবং কম্পোনেন্টের জন্য

namespace ModelView_MAUI
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
            set
            {
                _classSection = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsActiveChecked)); // অবজেক্ট চেঞ্জ হলে চেকবক্সও আপডেট হবে
            }
        }

        // MAUI CheckBox-এর জন্য string ("0"/"1") থেকে bool ম্যাপিং প্রপার্টি
        public bool IsActiveChecked
        {
            get => ClassSection?.Activate == "1";
            set
            {
                if (ClassSection != null)
                {
                    ClassSection.Activate = value ? "1" : "0";
                    OnPropertyChanged();
                }
            }
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
                    // লিস্ট থেকে সিলেক্ট করলে ডাটা ক্লোন করে বক্সে পাঠানো হচ্ছে
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

        // MAUI Command ব্যবহার করা হয়েছে (WPF RelayCommand এর পরিবর্তে)
        public Command AddCommand { get; }
        public Command SaveCommand { get; }
        public Command ActiveClickCommand { get; }
        public Command DeleteCommand { get; }
        public Command EditCommand { get; }
        public Command ExitCommand { get; }

        // XAML প্রিভিউ এবং এরর এড়ানোর জন্য ডিফল্ট কনস্ট্রাক্টর
        public ClassSectionViewModel() : this(string.Empty)
        {
        }

        // মূল কনস্ট্রাক্টর
        public ClassSectionViewModel(string uid)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("http://74.208.227.170:8050/") };

            ClassSection.EntryByUid = uid;

            AddCommand = new Command(OnAdd);
            SaveCommand = new Command(async () => await OnSave());
            ActiveClickCommand = new Command(OnActive);
            DeleteCommand = new Command(async () => await OnDelete());
            EditCommand = new Command(() => { /* এডিট মোড লজিক */ });
            ExitCommand = new Command(OnExit);

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
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private async void OnAdd()
        {
            string currentUid = ClassSection.EntryByUid;
            ClassSection = new ClassSection { EntryByUid = currentUid };

            ClassSection.EntryDate = DateTime.Now;
            ClassSection.StartDate = DateTime.Now;
            ClassSection.Activate = "0";

            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Information", "নতুন ক্লাস সেকশন এন্ট্রি করার জন্য তৈরি।", "OK");
        }

        private async void OnActive()
        {
            if (ClassSection != null)
            {
                ClassSection.EndDate = DateTime.Now;
                ClassSection.Activate = "1";

                OnPropertyChanged(nameof(ClassSection));
                OnPropertyChanged(nameof(IsActiveChecked)); // UI চেকবক্স রিফ্রেশ করার জন্য

                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Success", "সেকশনটি Active হিসেবে সেট করা হয়েছে।", "OK");
            }
        }

        private async Task OnSave()
        {
            if (Application.Current?.MainPage == null) return;

            if (string.IsNullOrEmpty(ClassSection.SectionId))
            {
                await Application.Current.MainPage.DisplayAlert("Warning", "দয়া করে Section ID প্রদান করুন।", "OK");
                return;
            }

            ClassSection.LastUpdate = DateTime.Now;
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Data/SaveSection", ClassSection);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "ডেটাবেসে সফলভাবে সেভ করা হয়েছে!", "OK");
                    await LoadInitialData();
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", $"সেভ করা সম্ভব হয়নি।\nসার্ভার মেসেজ: {errorMsg}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Connection Error", ex.Message, "OK");
            }
        }

        private async Task OnDelete()
        {
            if (Application.Current?.MainPage == null) return;

            if (SelectedClassSection == null)
            {
                await Application.Current.MainPage.DisplayAlert("Warning", "মুছে ফেলার জন্য আগে একটি সেকশন সিলেক্ট করুন।", "OK");
                return;
            }

            bool result = await Application.Current.MainPage.DisplayAlert("নিশ্চিতকরণ", "আপনি কি এই সেকশনটি মুছে ফেলতে চান?", "Yes", "No");
            if (result)
            {
                try
                {
                    var response = await _httpClient.DeleteAsync($"api/Data/DeleteSection/{SelectedClassSection.SectionId}");
                    if (response.IsSuccessStatusCode)
                    {
                        await Application.Current.MainPage.DisplayAlert("Success", "সফলভাবে মুছে ফেলা হয়েছে।", "OK");
                        await LoadInitialData();
                        ClassSection = new ClassSection();
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "সার্ভার থেকে মুছে ফেলা সম্ভব হয়নি।", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
                }
            }
        }

        private void OnExit()
        {
            // MAUI উইন্ডো ক্লোজ করার প্রসেস
            var window = Application.Current?.Windows.FirstOrDefault();
            if (window != null)
            {
                Application.Current.CloseWindow(window);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}