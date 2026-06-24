using System;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ModelView_WPF
{
    public class ClassSubject
    {
        public string SubjectCode { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
        public string? SubjectDesc { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? EntryByUid { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string? Activate { get; set; }
    }

    public class ClassSubjectViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private string _receivedUid; // UID সংরক্ষণের জন্য

        // ডাটাবেস থেকে আসা লিস্টটি রাখার জন্য এটি প্রয়োজন
        private ObservableCollection<ClassSubject> _subjectList;
        public ObservableCollection<ClassSubject> SubjectList
        {
            get => _subjectList;
            set { _subjectList = value; OnPropertyChanged(); }
        }

        private ClassSubject _currentSubject = new ClassSubject();
        public ClassSubject CurrentSubject
        {
            get => _currentSubject;
            set { _currentSubject = value; OnPropertyChanged(); }
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

        // ডিফল্ট কনস্ট্রাক্টর (XAML ডিজাইনারের জন্য)
        public ClassSubjectViewModel() : this(null)
        {
        }

        // কনস্ট্রাক্টর আপডেট করা হয়েছে UID রিসিভ করার জন্য
        public ClassSubjectViewModel(string uid)
        {
            _receivedUid = uid;
            _httpClient = new HttpClient { BaseAddress = new Uri("http://74.208.227.170:8050//") };
            SubjectList = new ObservableCollection<ClassSubject>();

            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
            AddCommand = new RelayCommand((obj) => ResetForm());
            DeleteCommand = new RelayCommand(async (obj) => await ExecuteDelete());

            ResetForm();
            _ = LoadSubjects(); // অ্যাপ চালু হলে ডাটা লোড হবে
        }

        // ডাটাবেস থেকে ডাটা রিফ্রেশ করার মেথড
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
            catch { /* API call logic error handle */ }
        }

        private async Task ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(CurrentSubject.SubjectCode))
            {
                MessageBox.Show("Please enter Subject Code.");
                return;
            }

            CurrentSubject.LastUpdate = DateTime.Now;
            // Admin এর পরিবর্তে রিসিভ করা UID ব্যবহার করা হচ্ছে
            CurrentSubject.EntryByUid = _receivedUid;

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Data/SaveSubject", CurrentSubject);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Class Subject Saved/Updated Successfully!");
                    await LoadSubjects(); // সেভ করার পর লিস্ট রিফ্রেশ হবে
                    ResetForm();
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Server Error: {response.StatusCode}\nDetails: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to API: " + ex.Message);
            }
        }

        private async Task ExecuteDelete()
        {
            if (string.IsNullOrWhiteSpace(CurrentSubject.SubjectCode))
            {
                MessageBox.Show("Please select or enter a Subject Code to delete.");
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete Subject: {CurrentSubject.SubjectCode}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var response = await _httpClient.DeleteAsync($"api/Data/DeleteSubject/{CurrentSubject.SubjectCode}");

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Deleted Successfully!");
                        await LoadSubjects(); // ডিলিট করার পর লিস্ট রিফ্রেশ হবে
                        ResetForm();
                    }
                    else
                    {
                        MessageBox.Show("Error deleting record.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("API Connection Error: " + ex.Message);
                }
            }
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
                EntryByUid = _receivedUid // রিসেট করার সময়ও UID ধরে রাখা হচ্ছে
            };
            IsActiveChecked = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}