using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Model_WPF;

namespace ModelView_WPF
{
    public class AccExpendHeadViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "api/AccExpendHead";

        // Collection for DataGrid
        private ObservableCollection<AccExpendHeadModel> _expenseList = new();
        public ObservableCollection<AccExpendHeadModel> ExpenseList
        {
            get => _expenseList;
            set { _expenseList = value; OnPropertyChanged(); }
        }

        // Selected item from DataGrid
        private AccExpendHeadModel? _selectedExpense;
        public AccExpendHeadModel? SelectedExpense
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
            // আপনার API পোর্ট অনুযায়ী URL চেক করে নিবেন (যেমন: 7073)
            _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7073/") };

            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(async (p) => await ExecuteSave());
            EditCommand = new RelayCommand(async (p) => await ExecuteEdit());
            ExitCommand = new RelayCommand(ExecuteExit);

            _ = LoadData(); // উইন্ডো লোড হওয়ার সময় ডেটা ফেচ করবে
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
                MessageBox.Show($"Data Load Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAdd(object? parameter)
        {
            ClearFields();
        }

        private async Task ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(ExpCode) || string.IsNullOrWhiteSpace(ExpDetail))
            {
                MessageBox.Show("Please fill all required fields (ExpCode and ExpDetail).", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                    MessageBox.Show("Saved Successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadData();
                    ClearFields();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error saving data: {error}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ExecuteEdit()
        {
            if (SelectedExpense == null)
            {
                MessageBox.Show("Please select an item from the list to update.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var updatedEntry = new AccExpendHeadModel
            {
                // Primary key/Code আপডেটযোগ্য না হলে আগেরটাই পাঠাবে
                ExpCode = this.ExpCode,
                ExpDetail = this.ExpDetail,
                UID = this.UID,
                EntryDate = this.EntryDate
            };

            try
            {
                // আপনার Controller-এর Put মেথড শুধু অবজেক্ট নিচ্ছে, তাই সরাসরি URL এ অবজেক্ট পাঠানো হচ্ছে
                var response = await _httpClient.PutAsJsonAsync(ApiUrl, updatedEntry);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Updated Successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadData();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Update failed. Please check if the data is correct.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Update Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteExit(object? parameter)
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

        private void ClearFields()
        {
            ExpCode = string.Empty;
            ExpDetail = string.Empty;
            UID = string.Empty;
            EntryDate = DateTime.Now;
            SelectedExpense = null;
        }

        #region PropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }
}