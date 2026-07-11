using System;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace View_MAUI // আপনার MAUI প্রজেক্টের Namespace অনুযায়ী পরিবর্তন করতে পারেন
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

        // ডাটাবেস থেকে আসা লিস্টটি রাখার জন্য
        private ObservableCollection<ClassSubject> _subjectList = new();
        public ObservableCollection<ClassSubject> SubjectList
        {
            get => _subjectList;
            set { _subjectList = value; OnPropertyChanged(); }
        }

        // ফর্মের ইনপুট ডাটা বাইন্ডিং-এর জন্য
        private ClassSubject _currentSubject = new();
        public ClassSubject CurrentSubject
        {
            get => _currentSubject;
            set { _currentSubject = value; OnPropertyChanged(); }
        }

        // XAML CollectionView-তে সিলেক্টেড আইটেম ধরার জন্য
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
                        // লিস্ট থেকে সিলেক্ট করলে ফর্মে ডাটা লোড হবে
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

        // XAML-এর সব বাটনের Commands
        public ICommand SaveCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand ToggleStatusCommand { get; }

        // ডিফল্ট কনস্ট্রাক্টর (XAML ডিজাইনারের জন্য)
        public ClassSubjectViewModel() : this(string.Empty)
        {
        }

        // মেইন কনস্ট্রাক্টর
        public ClassSubjectViewModel(string uid)
        {
            _receivedUid = uid;

            // অ্যান্ড্রয়েড এমুলেটর বা রিয়েল ডিভাইসের জন্য বেস অ্যাড্রেসটি লোকালহোস্ট হলে পরিবর্তন লাগতে পারে, 
            // তবে যেহেতু লাইভ আইপি তাই এটাই থাকবে।
            _httpClient = new HttpClient { BaseAddress = new Uri("http://74.208.227.170:8050/") };
            SubjectList = new ObservableCollection<ClassSubject>();

            // Command ইনিশিয়ালাইজেশন
            SaveCommand = new Command(async () => await ExecuteSave());
            AddCommand = new Command(ResetForm);
            DeleteCommand = new Command(async () => await ExecuteDelete());
            EditCommand = new Command(ExecuteEdit);
            ExitCommand = new Command(async () => await ExecuteExit());
            ToggleStatusCommand = new Command(ExecuteToggleStatus);

            ResetForm();
            _ = LoadSubjects();
        }

        // API থেকে ডাটা লোড করা
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

        // সেভ বা আপডেট করার মেথড
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

        // ডিলিট করার মেথড
        private async Task ExecuteDelete()
        {
            if (string.IsNullOrWhiteSpace(CurrentSubject.SubjectCode))
            {
                await ShowAlertAsync("Validation", "Please select or enter a Subject Code to delete.");
                return;
            }

            // MAUI Style Confirmation Alert
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

        // এক্সিট বাটন লজিক
        private async Task ExecuteExit()
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

        // এডিট বাটন লজিক (সিলেক্টেড আইটেম ফর্মে পুশ করা)
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

        // স্ট্যাটাস টগল বাটন লজিক
        private void ExecuteToggleStatus()
        {
            IsActiveChecked = !IsActiveChecked;
        }

        // লিস্ট থেকে সিলেক্ট করা আইটেম ফর্মের ইনপুটে নেওয়া
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

        // ফর্ম রিসেট করার মেথড
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

        // MAUI তে অ্যালার্ট দেখানোর হেল্পার মেথড
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

    // MAUI তে WPF এর RelayCommand এর জায়গায় আমরা বিল্ট-ইন 'Command' ব্যবহার করেছি, 
    // তাই আলাদা RelayCommand ক্লাস দরকার হবে না।
}