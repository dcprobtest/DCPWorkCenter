using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json; // Required for simple JSON Extensions like GetFromJsonAsync and PostAsJsonAsync
using System.Threading.Tasks;
using Model_WPF; // For using AccUSExpendHeadModel
using ModelView_WPF.Helpers;

namespace ModelView_WPF
{
    public class AccUSExpendHeadViewModel : INotifyPropertyChanged
    {
        #region Fields

        private string _expCode;
        private string _expDetail;
        private string _uid;
        private DateTime? _entryDate = DateTime.Now; // Default current date
        private ObservableCollection<AccUSExpendHeadModel> _expenseList;
        private AccUSExpendHeadModel _selectedExpense;

        // HttpClient field for API communication updated to use ApiSettings
        private readonly HttpClient _httpClient;

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

        public DateTime? EntryDate
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
                    // When a row is selected from the DataGrid, load it into the TextBoxes
                    ExpCode = _selectedExpense.ExpCode;
                    ExpDetail = _selectedExpense.ExpDetail;
                    UID = _selectedExpense.UID;
                    EntryDate = _selectedExpense.EntryDate;
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
        {
            // Initialize HttpClient with ApiSettings BaseUrl
            _httpClient = new HttpClient { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            // Initialize the list
            ExpenseList = new ObservableCollection<AccUSExpendHeadModel>();

            // Link commands with methods (using your RelayCommand)
            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(ExecuteSave);
            EditCommand = new RelayCommand(ExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            // Call method to load initial data from the database via API
            LoadExpenseData();
        }

        // Parameterized constructor to receive UID from View
        public AccUSExpendHeadViewModel(string uid) : this()
        {
            // Set the received UID to the property so it displays in the interface
            UID = uid;
        }

        #endregion

        #region Command Methods

        private void ExecuteAdd(object parameter)
        {
            // Clear fields for a new entry
            ExpCode = string.Empty;
            ExpDetail = string.Empty;
            UID = "U0001"; // Default User ID example, set according to your logic
            EntryDate = DateTime.Now;
            SelectedExpense = null;
        }

        private async void ExecuteSave(object parameter)
        {
            if (string.IsNullOrEmpty(ExpCode) || string.IsNullOrEmpty(ExpDetail))
            {
                MessageBox.Show("Please fill required fields!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                // Call API POST endpoint using ApiSettings
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(ApiSettings.AccUSExpendHeadBaseUrl, newItem);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Data Saved Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadExpenseData(); // Reload from database to ensure UI is synchronized
                    ExecuteAdd(null); // Refresh fields after saving
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    MessageBox.Show("An entry with the same ExpCode already exists.", "Conflict", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Server Error: {response.ReasonPhrase}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to API: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExecuteEdit(object parameter)
        {
            if (SelectedExpense == null)
            {
                MessageBox.Show("Please select an item from the list to edit.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                // Call API PUT endpoint using ApiSettings route helper
                HttpResponseMessage response = await _httpClient.PutAsJsonAsync(ApiSettings.GetAccUSExpendHeadByIdUrl(updatedItem.ExpCode), updatedItem);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Data Updated Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadExpenseData(); // Refresh the list to reflect updates
                    ExecuteAdd(null); // Reset form state
                }
                else
                {
                    MessageBox.Show($"Server Error: {response.ReasonPhrase}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to API: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteExit(object parameter)
        {
            // Logic to close the current window
            if (parameter is Window currentWindow)
            {
                currentWindow.Close();
            }
        }

        private async void LoadExpenseData()
        {
            try
            {
                // Call API GET endpoint using ApiSettings to fetch all expense records asynchronously
                var data = await _httpClient.GetFromJsonAsync<AccUSExpendHeadModel[]>(ApiSettings.AccUSExpendHeadBaseUrl);

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
                MessageBox.Show($"Failed to load data from API: {ex.Message}", "Data Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
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