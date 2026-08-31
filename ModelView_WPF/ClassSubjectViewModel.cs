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
using System.Linq;
using Model_WPF;
using ModelView_WPF.Helpers;

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
        private string _receivedUid;

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

        private ClassSubject _selectedSubject;
        public ClassSubject SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                _selectedSubject = value;
                OnPropertyChanged();
                if (_selectedSubject != null)
                {
                    CurrentSubject = new ClassSubject
                    {
                        SubjectCode = _selectedSubject.SubjectCode,
                        CourseCode = _selectedSubject.CourseCode,
                        SubjectDesc = _selectedSubject.SubjectDesc,
                        EntryDate = _selectedSubject.EntryDate,
                        StartDate = _selectedSubject.StartDate,
                        EndDate = _selectedSubject.EndDate,
                        EntryByUid = _selectedSubject.EntryByUid,
                        LastUpdate = _selectedSubject.LastUpdate,
                        Activate = _selectedSubject.Activate
                    };
                    IsActiveChecked = _selectedSubject.Activate == "1";
                    IsEditable = false;
                }
            }
        }

        private bool _isEditable;
        public bool IsEditable
        {
            get => _isEditable;
            set
            {
                _isEditable = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEditable));
            }
        }

        public bool IsNotEditable => !IsEditable;

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

        public ClassSubjectViewModel() : this(null)
        {
        }

        public ClassSubjectViewModel(string uid)
        {
            _receivedUid = uid;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };
            SubjectList = new ObservableCollection<ClassSubject>();

            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
            AddCommand = new RelayCommand((obj) => ExecuteAdd());
            DeleteCommand = new RelayCommand(async (obj) => await ExecuteDelete());
            EditCommand = new RelayCommand((obj) => ExecuteEdit());
            ExitCommand = new RelayCommand((obj) => ExecuteExit());
            ToggleStatusCommand = new RelayCommand((obj) => ExecuteToggleStatus());

            ResetForm();
            _ = LoadSubjects();
        }

        private async Task LoadSubjects()
        {
            try
            {
                var subjects = await _httpClient.GetFromJsonAsync<List<ClassSubject>>(ApiSettings.GetClassSubjectsUrl());
                if (subjects != null)
                {
                    SubjectList = new ObservableCollection<ClassSubject>(subjects);
                }
            }
            catch { }
        }

        private void ExecuteAdd()
        {
            SelectedSubject = null;
            IsEditable = true;

            CurrentSubject = new ClassSubject
            {
                SubjectCode = string.Empty,
                CourseCode = string.Empty,
                SubjectDesc = string.Empty,
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(3),
                Activate = "1",
                EntryByUid = _receivedUid
            };
            IsActiveChecked = false;

            if (SubjectList != null && SubjectList.Count > 0)
            {
                var maxCode = SubjectList
                    .Select(s => s.SubjectCode)
                    .Where(code => int.TryParse(code, out _))
                    .Select(code => int.Parse(code))
                    .DefaultIfEmpty(0)
                    .Max();

                CurrentSubject.SubjectCode = (maxCode + 1).ToString("D2");
            }
            else
            {
                CurrentSubject.SubjectCode = "01";
            }

            OnPropertyChanged(nameof(CurrentSubject));
        }

        private void ExecuteEdit()
        {
            if (SelectedSubject == null)
            {
                MessageBox.Show("Please select a subject from the list to edit.");
                return;
            }

            IsEditable = true;
        }

        private void ExecuteExit()
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

        private void ExecuteToggleStatus()
        {
            if (!IsEditable) return;
            IsActiveChecked = !IsActiveChecked;
            MessageBox.Show($"Status updated to: {(IsActiveChecked ? "Active" : "Inactive")}");
        }

        private async Task ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(CurrentSubject.SubjectCode))
            {
                MessageBox.Show("Please enter Subject Code.");
                return;
            }

            CurrentSubject.LastUpdate = DateTime.Now;
            if (string.IsNullOrEmpty(CurrentSubject.EntryByUid))
            {
                CurrentSubject.EntryByUid = _receivedUid;
            }

            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiSettings.SaveClassSubjectUrl(), CurrentSubject);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Class Subject Saved Successfully!");
                    await LoadSubjects();
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
                    var response = await _httpClient.DeleteAsync(ApiSettings.DeleteClassSubjectUrl(CurrentSubject.SubjectCode));

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Deleted Successfully!");
                        await LoadSubjects();
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
                Activate = "1",
                EntryByUid = _receivedUid
            };
            IsActiveChecked = false;
            IsEditable = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}