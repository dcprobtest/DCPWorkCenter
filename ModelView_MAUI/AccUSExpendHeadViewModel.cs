using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices; // Required for DeviceInfo
using Model_MAUI;

namespace View_MAUI
{
    public class AccUSExpendHeadViewModel : INotifyPropertyChanged
    {
        #region Fields

        private string _expCode;
        private string _expDetail;
        private string _uid;
        private DateTime _entryDate = DateTime.Now;
        private ObservableCollection<AccUSExpendHeadModel> _expenseList;
        private AccUSExpendHeadModel _selectedExpense;

        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl;

        #endregion

        #region Properties

        public string ExpCode
        {
            get => _expCode;
            set
            {
                _expCode = value;
                OnPropertyChanged();
            }
        }

        public string ExpDetail
        {
            get => _expDetail;
            set
            {
                _expDetail = value;
                OnPropertyChanged();
            }
        }

        public string UID
        {
            get => _uid;
            set
            {
                _uid = value;
                OnPropertyChanged();
            }
        }

        public DateTime EntryDate
        {
            get => _entryDate;
            set
            {
                _entryDate = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AccUSExpendHeadModel> ExpenseList
        {
            get => _expenseList;
            set
            {
                _expenseList = value;
                OnPropertyChanged();
            }
        }

        public AccUSExpendHeadModel SelectedExpense
        {
            get => _selectedExpense;
            set
            {
                _selectedExpense = value;
                OnPropertyChanged();
                if (_selectedExpense != null)
                {
                    // Load data into text fields when a row is selected from CollectionView
                    ExpCode = _selectedExpense.ExpCode;
                    ExpDetail = _selectedExpense.ExpDetail;
                    UID = _selectedExpense.UID;
                    EntryDate = _selectedExpense.EntryDate ?? DateTime.Now;
                }
            }
        }

        #endregion

        #region Commands

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        #endregion

        #region Constructors

        public AccUSExpendHeadViewModel()
            : this(string.Empty)
        {
        }

        // Parameterized Constructor
        public AccUSExpendHeadViewModel(string uid)
        {
            // Setup Base URL logic for Android Emulator access
            string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "https://10.0.2.2:7073/"
                : "https://localhost:7073/";

            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
            _apiBaseUrl = "api/AccUSExpendHead";

            ExpenseList = new ObservableCollection<AccUSExpendHeadModel>();

            // MAUI Core Commands
            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async () => await ExecuteSave());
            EditCommand = new Command(async () => await ExecuteEdit());
            ExitCommand = new Command(async () => await ExecuteExit());

            // Assign logged-in user ID
            UID = string.IsNullOrEmpty(uid) ? "U0001" : uid;

            // Load initial data
            LoadExpenseData();
        }

        #endregion

        #region Command Methods

        private void ExecuteAdd()
        {
            ExpCode = string.Empty;
            ExpDetail = string.Empty;
            EntryDate = DateTime.Now;
            SelectedExpense = null;
        }

        private async Task ExecuteSave()
        {
            if (string.IsNullOrEmpty(ExpCode) || string.IsNullOrEmpty(ExpDetail))
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Validation Error", "Please fill required fields!", "OK");
                }
                return;
            }

            var newItem = new AccUSExpendHeadModel
            {
                ExpCode = this.ExpCode,
                ExpDetail = this.ExpDetail,
                UID = this.UID,
                EntryDate = this.EntryDate
            };

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl, newItem);

                if (response.IsSuccessStatusCode)
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Success", "Data Saved Successfully!", "OK");
                    }
                    LoadExpenseData();
                    ExecuteAdd();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Conflict", "An entry with the same ExpCode already exists.", "OK");
                    }
                }
                else
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", $"Server Error: {response.ReasonPhrase}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Connection Error", $"Failed to connect to API: {ex.Message}", "OK");
                }
            }
        }

        private async Task ExecuteEdit()
        {
            if (SelectedExpense == null)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Selection Required", "Please select an item from the list to edit.", "OK");
                }
                return;
            }

            var updatedItem = new AccUSExpendHeadModel
            {
                ExpCode = this.ExpCode,
                ExpDetail = this.ExpDetail,
                UID = this.UID,
                EntryDate = this.EntryDate
            };

            try
            {
                HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"{_apiBaseUrl}/{updatedItem.ExpCode}", updatedItem);

                if (response.IsSuccessStatusCode)
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Success", "Data Updated Successfully!", "OK");
                    }
                    LoadExpenseData();
                    ExecuteAdd();
                }
                else
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", $"Server Error: {response.ReasonPhrase}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Connection Error", $"Failed to connect to API: {ex.Message}", "OK");
                }
            }
        }

        private async Task ExecuteExit()
        {
            // Pop current page from navigation stack
            if (Application.Current?.MainPage?.Navigation.NavigationStack.Count > 1)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

        private async void LoadExpenseData()
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<AccUSExpendHeadModel[]>(_apiBaseUrl);

                if (data != null)
                {
                    ExpenseList.Clear();
                    foreach (var item in data)
                    {
                        ExpenseList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Data Load Error", $"Failed to load data from API: {ex.Message}", "OK");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}