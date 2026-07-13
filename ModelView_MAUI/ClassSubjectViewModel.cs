using System;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Devices; // Required for DeviceInfo
using Microsoft.Maui.Controls;

namespace View_MAUI
{
    public class ClassSubject : INotifyPropertyChanged
    {
        private string _subjectCode = string.Empty;
        public string SubjectCode
        {
            get => _subjectCode;
            set { _subjectCode = value; OnPropertyChanged(); }
        }

        private string? _courseCode;
        public string? CourseCode
        {
            get => _courseCode;
            set { _courseCode = value; OnPropertyChanged(); }
        }

        private string? _subjectDesc;
        public string? SubjectDesc
        {
            get => _subjectDesc;
            set { _subjectDesc = value; OnPropertyChanged(); }
        }

        private DateTime? _entryDate;
        public DateTime? EntryDate
        {
            get => _entryDate;
            set { _entryDate = value; OnPropertyChanged(); }
        }

        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); }
        }

        private string? _entryByUid;
        public string? EntryByUid
        {
            get => _entryByUid;
            set { _entryByUid = value; OnPropertyChanged(); }
        }

        private DateTime? _lastUpdate;
        public DateTime? LastUpdate
        {
            get => _lastUpdate;
            set { _lastUpdate = value; OnPropertyChanged(); }
        }

        private string? _activate;
        public string? Activate
        {
            get => _activate;
            set { _activate = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ClassSubjectViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private string _receivedUid;

        private ObservableCollection<ClassSubject> _subjectList = new();
        public ObservableCollection<ClassSubject> SubjectList
        {
            get => _subjectList;
            set { _subjectList = value; OnPropertyChanged(); }
        }

        private ClassSubject _currentSubject = new();
        public ClassSubject CurrentSubject
        {
            get => _currentSubject;
            set { _currentSubject = value; OnPropertyChanged(); }
        }

        private ClassSubject? _selectedSubject;
        public ClassSubject? SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                if (_selectedSubject != value)
                {
                    _selectedSubject = value;
                    OnPropertyChanged();
                    if (_selectedSubject != null)
                    {
                        LoadSelectedSubjectToForm(_selectedSubject);
                    }
                }
            }
        }

        private bool _isActiveChecked;
        public bool IsActiveChecked
        {
            get => _isActiveChecked;
            set
            {
                if (_isActiveChecked != value)
                {
                    _isActiveChecked = value;
                    CurrentSubject.Activate = value ? "1" : "0";
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ToggleStatusCommand { get; }

        public ClassSubjectViewModel() : this(string.Empty)
        {
        }

        public ClassSubjectViewModel(string uid)
        {
            _receivedUid = uid;

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
            SubjectList = new ObservableCollection<ClassSubject>();

            SaveCommand = new Command(async () => await ExecuteSave());
            AddCommand = new Command(ResetForm);
            DeleteCommand = new Command(async () => await ExecuteDelete());
            EditCommand = new Command(ExecuteEdit);
            ExitCommand = new Command(async () => await ExecuteExit());
            ToggleStatusCommand = new Command(ExecuteToggleStatus);

            ResetForm();
            _ = LoadSubjects();
        }

        private async Task LoadSubjects()
        {
            try
            {
                var subjects = await _httpClient.GetFromJsonAsync<List<ClassSubject>>("api/Data/GetSubjects");
                if (subjects != null)
                {
                    SubjectList = new ObservableCollection<ClassSubject>(subjects);
                }
            }
            catch
            {
                await ShowAlertAsync("Error", "Failed to load subjects from server.");
            }
        }

        private async Task ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(CurrentSubject.SubjectCode))
            {
                await ShowAlertAsync("Validation", "Please enter Subject Code.");
                return;
            }

            CurrentSubject.LastUpdate = DateTime.Now;
            CurrentSubject.EntryByUid = _receivedUid;

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Data/SaveSubject", CurrentSubject);

                if (response.IsSuccessStatusCode)
                {
                    await ShowAlertAsync("Success", "Class Subject Saved/Updated Successfully!");
                    await LoadSubjects();
                    ResetForm();
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    await ShowAlertAsync("Server Error", $"Status: {response.StatusCode}\nDetails: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                await ShowAlertAsync("Connection Error", "Could not connect to API: " + ex.Message);
            }
        }

        private async Task ExecuteDelete()
        {
            if (string.IsNullOrWhiteSpace(CurrentSubject.SubjectCode))
            {
                await ShowAlertAsync("Validation", "Please select or enter a Subject Code to delete.");
                return;
            }

            bool confirm = await Application.Current!.MainPage!.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete Subject: {CurrentSubject.SubjectCode}?",
                "Yes", "No");

            if (confirm)
            {
                try
                {
                    var response = await _httpClient.DeleteAsync($"api/Data/DeleteSubject/{CurrentSubject.SubjectCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        await ShowAlertAsync("Success", "Deleted Successfully!");
                        await LoadSubjects();
                        ResetForm();
                    }
                    else
                    {
                        await ShowAlertAsync("Error", "Error deleting record.");
                    }
                }
                catch (Exception ex)
                {
                    await ShowAlertAsync("API Error", "API Connection Error: " + ex.Message);
                }
            }
        }

        private async Task ExecuteExit()
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

        private void ExecuteEdit()
        {
            if (SelectedSubject != null)
            {
                LoadSelectedSubjectToForm(SelectedSubject);
            }
            else
            {
                ShowAlertAsync("Notice", "Please select a subject from the list to edit.").ConfigureAwait(false);
            }
        }

        private void ExecuteToggleStatus()
        {
            IsActiveChecked = !IsActiveChecked;
        }

        private void LoadSelectedSubjectToForm(ClassSubject subject)
        {
            CurrentSubject = new ClassSubject
            {
                SubjectCode = subject.SubjectCode,
                CourseCode = subject.CourseCode,
                SubjectDesc = subject.SubjectDesc,
                EntryDate = subject.EntryDate,
                StartDate = subject.StartDate,
                EndDate = subject.EndDate,
                Activate = subject.Activate,
                EntryByUid = subject.EntryByUid,
                LastUpdate = subject.LastUpdate
            };
            IsActiveChecked = subject.Activate == "1";
        }

        private void ResetForm()
        {
            CurrentSubject = new ClassSubject
            {
                SubjectCode = string.Empty,
                CourseCode = string.Empty,
                SubjectDesc = string.Empty,
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(3),
                Activate = "0",
                EntryByUid = _receivedUid
            };
            IsActiveChecked = false;
            SelectedSubject = null;
        }

        private async Task ShowAlertAsync(string title, string message)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(title, message, "OK");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null!) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}