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
using ModelView_WPF.Helpers;

namespace ModelView_WPF
{
    public class AccExpendHeadViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private readonly string _receivedUid = string.Empty; // Local field to hold the received uid

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
                    // Populating fields when a row is selected from DataGrid
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
            // Using ApiSettings.BaseUrl directly in HttpClient
            _httpClient = new HttpClient { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(async (p) => await ExecuteSave());
            EditCommand = new RelayCommand(async (p) => await ExecuteEdit());
            ExitCommand = new RelayCommand(ExecuteExit);

            _ = LoadData(); // Fetches data when the window loads
        }

        // New constructor to receive uid from frmAccExpendHead.xaml.cs
        public AccExpendHeadViewModel(string uid) : this()
        {
            _receivedUid = uid;
            UID = uid; // Set received uid initially to show in the form
        }

        private async Task LoadData()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<AccExpendHeadModel>>(ApiSettings.AccExpendHeadBaseUrl);
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
                var response = await _httpClient.PostAsJsonAsync(ApiSettings.AccExpendHeadBaseUrl, newEntry);
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
                // If primary key/Code is not updatable, it sends the previous one
                ExpCode = this.ExpCode,
                ExpDetail = this.ExpDetail,
                UID = this.UID,
                EntryDate = this.EntryDate
            };

            try
            {
                // Since your controller's Put method only takes an object, the object is passed directly to the URL
                var response = await _httpClient.PutAsJsonAsync(ApiSettings.AccExpendHeadBaseUrl, updatedEntry);

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
            UID = _receivedUid; // Retains the received ID instead of clearing it when resetting fields
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