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
using Model_WPF;
using ModelView_WPF.Helpers;

namespace ModelView_WPF
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
                    // Cloning data when selected from the list to avoid affecting the original list before saving
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

        // Default constructor added to remove XAML errors
        public ClassSectionViewModel() : this(null)
        {
        }

        // Constructor updated to receive UID
        public ClassSectionViewModel(string uid)
        {
            // Localhost self-signed SSL certificate error bypass handler setup
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            // HttpClient configuration updated with ApiSettings.BaseUrl
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            // Setting EntryByUid from the passed parameter
            ClassSection.EntryByUid = uid;

            AddCommand = new RelayCommand(obj => OnAdd());
            SaveCommand = new RelayCommand(async obj => await OnSave());
            ActiveClickCommand = new RelayCommand(obj => OnActive());
            DeleteCommand = new RelayCommand(async obj => await OnDelete());
            EditCommand = new RelayCommand(obj => OnEdit());
            ExitCommand = new RelayCommand(obj => OnExit());

            _ = LoadInitialData();
        }

        private async Task LoadInitialData()
        {
            try
            {
                // 1. Load Course List using ApiSettings endpoint
                var courses = await _httpClient.GetFromJsonAsync<List<object>>(ApiSettings.GetClassSectionCoursesUrl());
                if (courses != null)
                {
                    CourseList.Clear();
                    foreach (var c in courses) CourseList.Add(c);
                }

                // 2. Load Section List using ApiSettings endpoint
                var sections = await _httpClient.GetFromJsonAsync<List<ClassSection>>(ApiSettings.GetClassSectionsUrl());
                if (sections != null)
                {
                    ClassSectionList.Clear();
                    foreach (var s in sections) ClassSectionList.Add(s);
                }
            }
            catch (Exception ex)
            {
                // Helpful error message if data loading fails
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        private void OnAdd()
        {
            // Preserving previous EntryByUid while creating a new object
            string currentUid = ClassSection.EntryByUid;
            ClassSection = new ClassSection();
            ClassSection.EntryByUid = currentUid;

            ClassSection.EntryDate = DateTime.Now;
            ClassSection.StartDate = DateTime.Now;
            ClassSection.Activate = "0";
            MessageBox.Show("Ready to enter a new class section.");
        }

        private void OnActive()
        {
            if (ClassSection != null)
            {
                ClassSection.EndDate = DateTime.Now;
                ClassSection.Activate = "1";
                // Since inner properties of ClassSection changed, notify update for the whole object
                OnPropertyChanged(nameof(ClassSection));
                MessageBox.Show("Section has been set as Active.");
            }
        }

        private void OnEdit()
        {
            if (SelectedClassSection == null)
            {
                MessageBox.Show("Please select a section from the list to edit.");
                return;
            }

            // Copying selected section values to active editable object
            ClassSection = new ClassSection
            {
                SectionId = SelectedClassSection.SectionId,
                SubjectCode = SelectedClassSection.SubjectCode,
                SectionDesc = SelectedClassSection.SectionDesc,
                EntryDate = SelectedClassSection.EntryDate,
                StartDate = SelectedClassSection.StartDate,
                EndDate = SelectedClassSection.EndDate,
                EntryByUid = SelectedClassSection.EntryByUid,
                LastUpdate = SelectedClassSection.LastUpdate,
                Activate = SelectedClassSection.Activate
            };

            MessageBox.Show("Section is now in Edit mode.");
        }

        private async Task OnSave()
        {
            if (string.IsNullOrEmpty(ClassSection.SectionId))
            {
                MessageBox.Show("Please provide a Section ID.");
                return;
            }

            ClassSection.LastUpdate = DateTime.Now;
            try
            {
                // Calling API Controller's SaveSection method using ApiSettings endpoint
                var response = await _httpClient.PostAsJsonAsync(ApiSettings.SaveClassSectionUrl(), ClassSection);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Successfully saved to database!");
                    await LoadInitialData(); // Reload to update the list
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Could not save. Status Code: {response.StatusCode}\nServer Message: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection Error: {ex.Message}");
            }
        }

        private async Task OnDelete()
        {
            if (SelectedClassSection == null)
            {
                MessageBox.Show("Please select a section first to delete.");
                return;
            }

            var result = MessageBox.Show("Are you sure you want to delete this section?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Calling API Controller's DeleteSection method using ApiSettings endpoint
                    var response = await _httpClient.DeleteAsync(ApiSettings.DeleteClassSectionUrl(SelectedClassSection.SectionId));
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Successfully deleted.");
                        await LoadInitialData();
                        ClassSection = new ClassSection(); // Clear input fields
                    }
                    else
                    {
                        MessageBox.Show("Could not delete from server.");
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