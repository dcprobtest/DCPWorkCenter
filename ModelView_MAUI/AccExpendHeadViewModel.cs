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
using Model_MAUI; // আপনার প্রজেক্টের সঠিক মডেল নেমস্পেস নিশ্চিত করুন

namespace ModelView_MAUI
{
    public class AccExpendHeadViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "api/AccExpendHead";
        private string _receivedUid = string.Empty;

        // Collection for CollectionView
        private ObservableCollection<AccExpendHeadModel> _expenseList = new();
        public ObservableCollection<AccExpendHeadModel> ExpenseList
        {
            get => _expenseList;
            set { _expenseList = value; OnPropertyChanged(); }
        }

        // Selected item from CollectionView
        private AccExpendHeadModel _selectedExpense;
        public AccExpendHeadModel _selectedExpenseItem
        {
            get => _selectedExpense;
            set
            {
                _selectedExpense = value;
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
            _httpClient = new HttpClient { BaseAddress = new Uri("http://74.208.227.170:8050/") };

            // MAUI Standard Commands
            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async () => await ExecuteSave());
            EditCommand = new Command(async () => await ExecuteEdit());
            ExitCommand = new Command(async () => await ExecuteExit());

            _ = LoadData();
        }

        // ফ্রন্ট-এন্ড ফর্ম ওপেনিং বা লগইন স্টেট থেকে UID পাস করার জন্য ওভারলোডেড কনস্ট্রাক্টর
        public AccExpendHeadViewModel(string uid) : this()
        {
            _receivedUid = uid;
            UID = uid;
        }

        private async Task LoadData()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<AccExpendHeadModel>>(ApiUrl);
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
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Warning", "Please fill all required fields (ExpCode and ExpDetail).", "OK");
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
                var response = await _httpClient.PostAsJsonAsync(ApiUrl, newEntry);
                if (response.IsSuccessStatusCode)
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Success", "Saved Successfully", "OK");

                    await LoadData();
                    ClearFields();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("API Error", $"Error saving data: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Error", $"Connection Error: {ex.Message}", "OK");
            }
        }

        private async Task ExecuteEdit()
        {
            if (_selectedExpense == null)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Selection Required", "Please select an item from the list to update.", "OK");
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
                var response = await _httpClient.PutAsJsonAsync(ApiUrl, updatedEntry);

                if (response.IsSuccessStatusCode)
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Success", "Updated Successfully", "OK");

                    await LoadData();
                    ClearFields();
                }
                else
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Error", "Update failed. Please check if the data is correct.", "OK");
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
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
            _selectedExpense = null;
            OnPropertyChanged(nameof(_selectedExpenseItem));
        }

        #region PropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}