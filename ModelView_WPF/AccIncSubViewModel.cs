using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Model_WPF;
using ModelView_WPF.Helpers;

namespace ModelView_WPF
{
    public class AccIncSubViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private bool _isEntryVisible;
        private AccIncSubModel _currentEntry;
        private AccIncSubModel _selectedIncSub;
        private string _selectedIncCode;
        private string _userUID; // Received User UID stored here

        // Collections for UI Binding
        public ObservableCollection<AccIncSubModel> IncSubList { get; set; }
        public ObservableCollection<string> IncCodeList { get; set; }

        // Commands
        public ICommand AddCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        // Default constructor
        public AccIncSubViewModel()
        {
            // API Base URL Setup updated to use ApiSettings
            _httpClient = new HttpClient { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            IncSubList = new ObservableCollection<AccIncSubModel>();
            IncCodeList = new ObservableCollection<string>();
            CurrentEntry = new AccIncSubModel();

            // Initialize Commands using existing RelayCommand
            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(async (param) => await ExecuteSave());
            EditCommand = new RelayCommand(ExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            // Default State
            IsEntryVisible = false;

            // Load ComboBox data from API asynchronously
            _ = LoadInitialData();
        }

        // Parameterized constructor to receive UID from Form
        public AccIncSubViewModel(string uid) : this()
        {
            this._userUID = uid;
            if (CurrentEntry != null)
            {
                CurrentEntry.UID = uid; // Sets UID for the initial form state if needed
            }
        }

        public bool IsEntryVisible
        {
            get => _isEntryVisible;
            set { _isEntryVisible = value; OnPropertyChanged(); }
        }

        public AccIncSubModel CurrentEntry
        {
            get => _currentEntry;
            set { _currentEntry = value; OnPropertyChanged(); }
        }

        public AccIncSubModel SelectedIncSub
        {
            get => _selectedIncSub;
            set
            {
                _selectedIncSub = value;
                OnPropertyChanged();
            }
        }

        public string SelectedIncCode
        {
            get => _selectedIncCode;
            set
            {
                _selectedIncCode = value;
                OnPropertyChanged();
                if (CurrentEntry != null)
                {
                    CurrentEntry.IncCode = value;
                }
            }
        }

        private void ExecuteAdd(object parameter)
        {
            CurrentEntry = new AccIncSubModel
            {
                EntryDate = DateTime.Now,
                UID = _userUID // Saved UID assigned here instead of default "1"
            };
            SelectedIncCode = string.Empty;
            IsEntryVisible = true;
        }

        private async Task ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(CurrentEntry.IncSubCode))
            {
                MessageBox.Show("IncSub Code is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // API endpoint call updated to use ApiSettings
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(ApiSettings.AccIncSubBaseUrl, CurrentEntry);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Data saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Add to list and clear form
                    IncSubList.Add(CurrentEntry);
                    CurrentEntry = new AccIncSubModel { UID = _userUID }; // Keeps UID ready for next entry
                    SelectedIncCode = string.Empty;
                    IsEntryVisible = false;
                }
                else
                {
                    MessageBox.Show($"Failed to save record. Status: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteEdit(object parameter)
        {
            if (parameter is AccIncSubModel selectedRow)
            {
                CurrentEntry = new AccIncSubModel
                {
                    IncSubCode = selectedRow.IncSubCode,
                    IncCode = selectedRow.IncCode,
                    IncDetail = selectedRow.IncDetail,
                    UID = selectedRow.UID,
                    EntryDate = selectedRow.EntryDate
                };
                SelectedIncCode = selectedRow.IncCode ?? string.Empty;
                IsEntryVisible = true;
            }
            else
            {
                MessageBox.Show("Please select an item from the list to edit.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteExit(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }

        private async Task LoadInitialData()
        {
            try
            {
                // Fetch dynamic IncCode list from the API endpoint using ApiSettings
                var codes = await _httpClient.GetFromJsonAsync<List<string>>(ApiSettings.AccIncSubGetIncomeHeadCodesUrl);
                if (codes != null)
                {
                    IncCodeList.Clear();
                    foreach (var code in codes)
                    {
                        IncCodeList.Add(code);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load Income Head Codes from API: {ex.Message}", "API Connection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}