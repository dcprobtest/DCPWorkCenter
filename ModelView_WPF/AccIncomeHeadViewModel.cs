using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Model_WPF; // References the Model project

namespace ModelView_WPF
{
    public class AccIncomeHeadViewModel : INotifyPropertyChanged
    {
        private string _incCode;
        private string _incDetail;
        private string _uid;
        private DateTime? _entryDate = DateTime.Now;
        private AccIncomeHeadModel _selectedIncome;

        // API Endpoint Configuration
        private static readonly HttpClient httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7073/") };

        // Collection to hold database records for the DataGrid
        public ObservableCollection<AccIncomeHeadModel> IncomeList { get; set; }

        public AccIncomeHeadViewModel()
        {
            IncomeList = new ObservableCollection<AccIncomeHeadModel>();

            // Initializing Commands using the existing RelayCommand in your project
            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            // Load existing data from the database via API when initialized
            LoadInitialData();
        }

        // Parameterized constructor to receive UID from Form
        public AccIncomeHeadViewModel(string uid) : this()
        {
            this.UID = uid; // Sets received UID to show on the form
        }

        #region Properties Linked to View Fields

        public string IncCode
        {
            get => _incCode;
            set
            {
                _incCode = value;
                OnPropertyChanged();
            }
        }

        public string IncDetail
        {
            get => _incDetail;
            set
            {
                _incDetail = value;
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

        public AccIncomeHeadModel SelectedIncome
        {
            get => _selectedIncome;
            set
            {
                _selectedIncome = value;
                OnPropertyChanged();
                if (_selectedIncome != null)
                {
                    // Populating fields when a row is selected from DataGrid
                    IncCode = _selectedIncome.IncCode;
                    IncDetail = _selectedIncome.IncDetail;
                    UID = _selectedIncome.UID;
                    EntryDate = _selectedIncome.EntryDate;
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

        #region Command Implementation Logic

        // Fetch data from API and populate DataGrid
        private async void LoadInitialData()
        {
            try
            {
                var data = await httpClient.GetFromJsonAsync<ObservableCollection<AccIncomeHeadModel>>("api/AccIncomeHead");
                if (data != null)
                {
                    IncomeList.Clear();
                    foreach (var item in data)
                    {
                        IncomeList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load initial data from API: {ex.Message}", "Data Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ExecuteAdd(object parameter)
        {
            // Reset fields for a new entry setup
            IncCode = string.Empty;
            IncDetail = string.Empty;
            UID = this.UID; // Keep the currently received User ID instead of hardcoded "1"
            EntryDate = DateTime.Now;
        }

        private bool CanExecuteSave(object parameter)
        {
            // Validation: Save button is enabled only when IncCode is exactly 2 characters
            return !string.IsNullOrWhiteSpace(IncCode) && IncCode.Length == 2;
        }

        private async void ExecuteSave(object parameter)
        {
            try
            {
                var newIncome = new AccIncomeHeadModel
                {
                    IncCode = this.IncCode,
                    IncDetail = this.IncDetail,
                    UID = this.UID,
                    EntryDate = this.EntryDate
                };

                // HTTP POST request to API to insert into database
                HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/AccIncomeHead", newIncome);

                if (response.IsSuccessStatusCode)
                {
                    IncomeList.Add(newIncome);
                    MessageBox.Show("Data saved successfully through API endpoint.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ExecuteAdd(null); // Clear fields after a successful save operation
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"API Error: {errorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteEdit(object parameter)
        {
            // Edit button remains enabled only when an existing grid row is selected
            return SelectedIncome != null;
        }

        private async void ExecuteEdit(object parameter)
        {
            try
            {
                if (SelectedIncome != null)
                {
                    SelectedIncome.IncCode = this.IncCode;
                    SelectedIncome.IncDetail = this.IncDetail;
                    SelectedIncome.UID = this.UID;
                    SelectedIncome.EntryDate = this.EntryDate;

                    // HTTP PUT request to API to update database row using IncCode as key
                    HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/AccIncomeHead/{IncCode}", SelectedIncome);

                    if (response.IsSuccessStatusCode)
                    {
                        // Refreshing collection bound to DataGrid layout
                        int index = IncomeList.IndexOf(SelectedIncome);
                        if (index >= 0)
                        {
                            IncomeList[index] = SelectedIncome;
                        }

                        MessageBox.Show("Data saved successfully.", "Updated", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"API Error: {errorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to modify record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteExit(object parameter)
        {
            // Closes the active window instance cleanly using window reference passed from CommandParameter
            if (parameter is Window window)
            {
                window.Close();
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