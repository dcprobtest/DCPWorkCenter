using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices; // Required for DeviceInfo

namespace ModelView_MAUI
{
    // Model class
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

    // ViewModel class
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
                OnPropertyChanged(nameof(IsActiveChecked)); // Updates checkbox when object changes
            }
        }

        // Property for MAUI CheckBox mapping from string ("0"/"1") to bool
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
                    // Clone data to the box when selected from the list
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

        // MAUI Commands
        public Command AddCommand { get; }
        public Command SaveCommand { get; }
        public Command ActiveClickCommand { get; }
        public Command DeleteCommand { get; }
        public Command EditCommand { get; }
        public Command ExitCommand { get; }

        // Default constructor
        public ClassSectionViewModel() : this(string.Empty)
        {
        }

        // Main constructor
        public ClassSectionViewModel(string uid)
        {
            // Using 10.0.2.2 for Android Emulator to access local machine
            string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "https://10.0.2.2:7073/"
                : "https://localhost:7073/";

            // Handler to ignore SSL certificate validation for local development
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };

            ClassSection.EntryByUid = uid;

            AddCommand = new Command(OnAdd);
            SaveCommand = new Command(async () => await OnSave());
            ActiveClickCommand = new Command(OnActive);
            DeleteCommand = new Command(async () => await OnDelete());
            EditCommand = new Command(() => { /* Edit mode logic */ });
            ExitCommand = new Command(OnExit);

            _ = LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            try
            {
                // 1. Load course list
                var courses = await _httpClient.GetFromJsonAsync<List<object>>("api/Data/GetCourses");
                if (courses != null)
                {
                    CourseList.Clear();
                    foreach (var c in courses) CourseList.Add(c);
                }

                // 2. Load section list
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
                await Application.Current.MainPage.DisplayAlert("Information", "Ready for new class section entry.", "OK");
        }

        private async void OnActive()
        {
            if (ClassSection != null)
            {
                ClassSection.EndDate = DateTime.Now;
                ClassSection.Activate = "1";

                OnPropertyChanged(nameof(ClassSection));
                OnPropertyChanged(nameof(IsActiveChecked)); // Refresh UI checkbox

                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Success", "Section set to Active.", "OK");
            }
        }

        private async Task OnSave()
        {
            if (Application.Current?.MainPage == null) return;

            if (string.IsNullOrEmpty(ClassSection.SectionId))
            {
                await Application.Current.MainPage.DisplayAlert("Warning", "Please provide Section ID.", "OK");
                return;
            }

            ClassSection.LastUpdate = DateTime.Now;
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Data/SaveSection", ClassSection);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Saved successfully to database!", "OK");
                    await LoadInitialData();
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", $"Could not save.\nServer message: {errorMsg}", "OK");
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
                await Application.Current.MainPage.DisplayAlert("Warning", "Select a section to delete.", "OK");
                return;
            }

            bool result = await Application.Current.MainPage.DisplayAlert("Confirmation", "Are you sure you want to delete this section?", "Yes", "No");
            if (result)
            {
                try
                {
                    var response = await _httpClient.DeleteAsync($"api/Data/DeleteSection/{SelectedClassSection.SectionId}");
                    if (response.IsSuccessStatusCode)
                    {
                        await Application.Current.MainPage.DisplayAlert("Success", "Deleted successfully.", "OK");
                        await LoadInitialData();
                        ClassSection = new ClassSection();
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Could not delete from server.", "OK");
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
            // Process to close MAUI window
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