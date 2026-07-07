using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Model_MAUI;

namespace View_MAUI
{
    public class AccExpendHeadViewModel : INotifyPropertyChanged
    {
        // Android Emulator এর জন্য 10.0.2.2 এবং পিসি বা ফিজিক্যাল ডিভাইসের জন্য localhost/IP
        private readonly string _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7073"
            : "https://localhost:7073";

        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private string _receivedUid = "Admin";

        private ObservableCollection<AccExpendHeadModel> _expenseList = new();
        public ObservableCollection<AccExpendHeadModel> ExpenseList
        {
            get => _expenseList;
            set { _expenseList = value; OnPropertyChanged(); }
        }

        private AccExpendHeadModel _selectedExpenseItem;
        public AccExpendHeadModel SelectedExpenseItem
        {
            get => _selectedExpenseItem;
            set
            {
                _selectedExpenseItem = value;
                OnPropertyChanged();
                if (value != null)
                {
                    ExpCode = value.ExpCode;
                    ExpDetail = value.ExpDetail;
                    UID = value.UID;
                    EntryDate = value.EntryDate ?? DateTime.Now;
                }
            }
        }

        #region Input Field Properties
        private string _expCode = string.Empty;
        public string ExpCode
        {
            get => _expCode;
            set { _expCode = value; OnPropertyChanged(); }
        }

        private string _expDetail = string.Empty;
        public string ExpDetail
        {
            get => _expDetail;
            set { _expDetail = value; OnPropertyChanged(); }
        }

        private string _uid = string.Empty;
        public string UID
        {
            get => _uid;
            set { _uid = value; OnPropertyChanged(); }
        }

        private DateTime _entryDate = DateTime.Now;
        public DateTime EntryDate
        {
            get => _entryDate;
            set { _entryDate = value; OnPropertyChanged(); }
        }
        #endregion

        #region Commands
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }
        #endregion

        public AccExpendHeadViewModel()
        {
            _httpClient = new HttpClient();
            _apiUrl = $"{_baseUrl}/api/AccExpendHead";

            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async () => await ExecuteSave());
            EditCommand = new Command(async () => await ExecuteEdit());
            ExitCommand = new Command(async () => await ExecuteExit());

            _ = LoadData();
        }

        // Updated constructor to receive UID properly
        public AccExpendHeadViewModel(string uid) : this()
        {
            _receivedUid = uid;
            UID = uid;
        }

        private async Task LoadData()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<AccExpendHeadModel>>(_apiUrl);
                if (response != null)
                {
                    ExpenseList = new ObservableCollection<AccExpendHeadModel>(response);
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Data Load Error: {ex.Message}", "OK");
                }
            }
        }

        private void ExecuteAdd()
        {
            ClearFields();
        }

        private async Task ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(ExpCode) || string.IsNullOrWhiteSpace(ExpDetail))
            {
                await Application.Current.MainPage.DisplayAlert("Warning", "Please fill all fields.", "OK");
                return;
            }

            var newEntry = new AccExpendHeadModel
            {
                ExpCode = this.ExpCode,
                ExpDetail = this.ExpDetail,
                UID = this.UID,
                EntryDate = this.EntryDate
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiUrl, newEntry);
                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Saved Successfully", "OK");
                    await LoadData();
                    ClearFields();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("API Error", "Error saving data", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Connection Error: {ex.Message}", "OK");
            }
        }

        private async Task ExecuteEdit()
        {
            if (SelectedExpenseItem == null)
            {
                await Application.Current.MainPage.DisplayAlert("Selection Required", "Please select an item.", "OK");
                return;
            }

            var updatedEntry = new AccExpendHeadModel
            {
                ExpCode = this.ExpCode,
                ExpDetail = this.ExpDetail,
                UID = this.UID,
                EntryDate = this.EntryDate
            };

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"{_apiUrl}/{SelectedExpenseItem.ExpCode}", updatedEntry);
                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Updated Successfully", "OK");
                    await LoadData();
                    ClearFields();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Update Error: {ex.Message}", "OK");
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

        private void ClearFields()
        {
            ExpCode = string.Empty;
            ExpDetail = string.Empty;
            UID = _receivedUid;
            EntryDate = DateTime.Now;
            SelectedExpenseItem = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}