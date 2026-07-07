using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices; 
using Model_MAUI;

namespace View_MAUI
{
    public class AccIncSubViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private bool _isEntryVisible;
        private AccIncSubModel _currentEntry;
        private AccIncSubModel _selectedIncSub;
        private string _selectedIncCode = string.Empty;
        private string _userUID = string.Empty;

        // Collections for UI Binding
        public ObservableCollection<AccIncSubModel> IncSubList { get; set; }
        public ObservableCollection<string> IncCodeList { get; set; }

        // MAUI standard commands
        public ICommand AddCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand ExitCommand { get; set; }

        // Default constructor
        public AccIncSubViewModel()
        {
            // URL logic updated to match AccEmpSalaryViewModel
            string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "https://10.0.2.2:7073/"
                : "https://localhost:7073/";

            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

            IncSubList = new ObservableCollection<AccIncSubModel>();
            IncCodeList = new ObservableCollection<string>();
            CurrentEntry = new AccIncSubModel { EntryDate = DateTime.Now };

            // MAUI Standard Command initialization
            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async () => await ExecuteSave());
            // Updated to handle async call properly
            EditCommand = new Command(async () => await ExecuteEdit());
            ExitCommand = new Command(async () => await ExecuteExit());

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
                CurrentEntry.UID = uid;
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
                if (_selectedIncSub != null)
                {
                    // Trigger edit logic when a row is selected from the CollectionView
                    _ = ExecuteEdit();
                }
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

        private void ExecuteAdd()
        {
            CurrentEntry = new AccIncSubModel
            {
                EntryDate = DateTime.Now,
                UID = _userUID
            };
            SelectedIncCode = string.Empty;
            IsEntryVisible = true;
        }

        private async Task ExecuteSave()
        {
            if (CurrentEntry == null || string.IsNullOrWhiteSpace(CurrentEntry.IncSubCode))
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Validation Error", "IncSub Code is required.", "OK");
                }
                return;
            }

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/AccIncSub", CurrentEntry);

                if (response.IsSuccessStatusCode)
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Success", "Data saved successfully.", "OK");
                    }

                    // Add to collection and clear input form
                    IncSubList.Add(CurrentEntry);
                    CurrentEntry = new AccIncSubModel { UID = _userUID, EntryDate = DateTime.Now };
                    SelectedIncCode = string.Empty;
                    IsEntryVisible = false;
                }
                else
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save record. Status: {response.StatusCode}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save record: {ex.Message}", "OK");
                }
            }
        }

        // Changed return type to Task and added async modifier
        private async Task ExecuteEdit()
        {
            if (SelectedIncSub != null)
            {
                CurrentEntry = new AccIncSubModel
                {
                    IncSubCode = SelectedIncSub.IncSubCode,
                    IncCode = SelectedIncSub.IncCode,
                    IncDetail = SelectedIncSub.IncDetail,
                    UID = SelectedIncSub.UID,
                    EntryDate = SelectedIncSub.EntryDate
                };
                SelectedIncCode = SelectedIncSub.IncCode ?? string.Empty;
                IsEntryVisible = true;
            }
            else
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Selection Required", "Please select an item from the list to edit.", "OK");
                }
            }
        }

        private async Task ExecuteExit()
        {
            if (Application.Current?.MainPage?.Navigation.NavigationStack.Count > 1)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            else
            {
                Application.Current?.Quit();
            }
        }

        private async Task LoadInitialData()
        {
            try
            {
                var codes = await _httpClient.GetFromJsonAsync<List<string>>("api/AccIncSub/IncomeHeadCodes");
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
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("API Connection Error", $"Failed to load Income Head Codes from API: {ex.Message}", "OK");
                }
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