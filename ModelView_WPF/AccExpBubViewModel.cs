using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Net.Http;
using ModelView_WPF;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using Model_WPF;
using ModelView_WPF.Helpers; // Namespace added for ApiSettings class

namespace ViewModel_WPF
{
    public class AccExpBubViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private readonly string _receivedUid = "Admin"; // Set "Admin" as default value

        #region Properties

        private AccExpBubModel _currentEntry;
        public AccExpBubModel CurrentEntry
        {
            get => _currentEntry;
            set { _currentEntry = value; OnPropertyChanged(); }
        }

        private ObservableCollection<AccExpBubModel> _expBubList;
        public ObservableCollection<AccExpBubModel> ExpBubList
        {
            get => _expBubList;
            set { _expBubList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<string> _expCodeList;
        public ObservableCollection<string> ExpCodeList
        {
            get => _expCodeList;
            set { _expCodeList = value; OnPropertyChanged(); }
        }

        private string _selectedExpCode;
        public string SelectedExpCode
        {
            get => _selectedExpCode;
            set
            {
                _selectedExpCode = value;
                if (CurrentEntry != null) CurrentEntry.ExpCode = value;
                OnPropertyChanged();
            }
        }

        private bool _isEntryVisible;
        public bool IsEntryVisible
        {
            get => _isEntryVisible;
            set { _isEntryVisible = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands
        public ICommand AddCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        #endregion

        public AccExpBubViewModel()
        {
            // ApiSettings.BaseUrl used directly in HttpClient
            _httpClient = new HttpClient { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            ExpBubList = new ObservableCollection<AccExpBubModel>();
            ExpCodeList = new ObservableCollection<string>(); // Will remain empty initially
            IsEntryVisible = false;

            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave(obj), CanExecuteAction);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteAction);
            ExitCommand = new RelayCommand(ExecuteExit);

            LoadDataFromApi();
            LoadExpCodes(); // This method has been newly added
        }

        // New constructor to receive uid from frmAccExpBub.xaml.cs
        public AccExpBubViewModel(string uid) : this()
        {
            _receivedUid = uid; // Set received uid to local variable
        }

        private async void LoadDataFromApi()
        {
            try
            {
                // Endpoint used from ApiSettings
                var data = await _httpClient.GetFromJsonAsync<ObservableCollection<AccExpBubModel>>(ApiSettings.AccExpBubBaseUrl);
                if (data != null) ExpBubList = data;
            }
            catch (Exception) { /* To avoid errors if API is not running */ }
        }

        // Method to load ExpCodes from API
        private async void LoadExpCodes()
        {
            try
            {
                // New endpoint being called from ApiSettings
                var codes = await _httpClient.GetFromJsonAsync<List<string>>(ApiSettings.AccExpBubGetExpendHeadCodesUrl);

                if (codes != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ExpCodeList.Clear();
                        foreach (var code in codes)
                        {
                            ExpCodeList.Add(code);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                // Handling if data does not arrive or an error occurs
            }
        }

        #region Command Methods

        private void ExecuteAdd(object obj)
        {
            IsEntryVisible = true;

            // 1. Auto ExpSubCode generation logic (001, 002 like that)
            string nextSubCode = "001";
            if (ExpBubList != null && ExpBubList.Any())
            {
                var lastCode = ExpBubList.Max(x => int.Parse(x.ExpSubCode));
                nextSubCode = (lastCode + 1).ToString("D3"); // Formats to 3 digits
            }

            // 2. New entry creation (Received _receivedUid set instead of hardcoded "Admin")
            CurrentEntry = new AccExpBubModel
            {
                EntryDate = DateTime.Now,
                UID = _receivedUid, // Received User ID will be set here and shown in the form
                ExpSubCode = nextSubCode,
                ExpDetail = ""
            };

            SelectedExpCode = null;
        }

        private async Task ExecuteSave(object obj)
        {
            if (CurrentEntry != null)
            {
                try
                {
                    // Endpoint used from ApiSettings
                    var response = await _httpClient.PostAsJsonAsync(ApiSettings.AccExpBubBaseUrl, CurrentEntry);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Data Saved Successfully!");
                        ExpBubList.Add(CurrentEntry);
                        IsEntryVisible = false;
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error: {error}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Connection Error: {ex.Message}");
                }
            }
        }

        private void ExecuteEdit(object obj)
        {
            if (IsEntryVisible == false) IsEntryVisible = true;
        }

        private void ExecuteExit(object obj)
        {
            Application.Current.Windows[0]?.Close();
        }

        private bool CanExecuteAction(object obj)
        {
            return IsEntryVisible;
        }

        #endregion

        #region PropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}