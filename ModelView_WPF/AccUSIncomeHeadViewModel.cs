using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json; // Required for Json extension methods
using Model_WPF; // References the project containing AccUSIncomeHeadModel

namespace ModelView_WPF
{
    public class AccUSIncomeHeadViewModel : INotifyPropertyChanged
    {
        // For keeping track of whether the current operation is an ADD or EDIT mode
        private bool _isEditMode = false;

        // HttpClient instance for API interaction
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "http://74.208.227.170:8050//api/AccUSIncomeHead";

        // Private field to retain the passed-in User ID across operations
        private readonly string _currentUID = string.Empty;

        #region Properties backed by fields

        private string _incCode;
        private string _incDetail;
        private string _uid;
        private DateTime? _entryDate;
        private AccUSIncomeHeadModel _selectedUSIncome;
        private ObservableCollection<AccUSIncomeHeadModel> _usIncomeList;

        public string IncCode
        {
            get => _incCode;
            set { _incCode = value; OnPropertyChanged(); }
        }

        public string IncDetail
        {
            get => _incDetail;
            set { _incDetail = value; OnPropertyChanged(); }
        }

        public string UID
        {
            get => _uid;
            set { _uid = value; OnPropertyChanged(); }
        }

        public DateTime? EntryDate
        {
            get => _entryDate;
            set { _entryDate = value; OnPropertyChanged(); }
        }

        public AccUSIncomeHeadModel SelectedUSIncome
        {
            get => _selectedUSIncome;
            set
            {
                _selectedUSIncome = value;
                OnPropertyChanged();

                // Automatically populate fields when an item is selected from the DataGrid
                if (_selectedUSIncome != null)
                {
                    IncCode = _selectedUSIncome.IncCode;
                    IncDetail = _selectedUSIncome.IncDetail;
                    UID = _selectedUSIncome.UID;
                    EntryDate = _selectedUSIncome.EntryDate;
                }
            }
        }

        public ObservableCollection<AccUSIncomeHeadModel> USIncomeList
        {
            get => _usIncomeList;
            set { _usIncomeList = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        #endregion

        #region Constructor

        // Default constructor
        public AccUSIncomeHeadViewModel()
        {
            // Initializing HttpClient
            _httpClient = new HttpClient();

            // Initializing the collection
            USIncomeList = new ObservableCollection<AccUSIncomeHeadModel>();

            // Binding commands using the RelayCommand available in your project
            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            // Setting up default initialization state
            ClearFields();
            LoadDataGridFromDatabase();
        }

        // Parameterized constructor to accept UID from the View layer
        public AccUSIncomeHeadViewModel(string uid) : this()
        {
            _currentUID = uid;
            this.UID = uid; // Displaying the passed UID in the form UI property
        }

        #endregion

        #region Command Methods Implementation

        private void ExecuteAdd(object parameter)
        {
            _isEditMode = false;
            ClearFields();

            // Set default runtime baseline data for new entries
            UID = _currentUID; // Dynamic UID assignment using the constructor value
            EntryDate = DateTime.Now;
        }

        private async void ExecuteSave(object parameter)
        {
            try
            {
                var inputData = new AccUSIncomeHeadModel
                {
                    IncCode = this.IncCode,
                    IncDetail = this.IncDetail,
                    UID = this.UID,
                    EntryDate = this.EntryDate
                };

                if (_isEditMode)
                {
                    // Update processing logic for existing records using HTTP PUT
                    HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/{IncCode}", inputData);

                    if (response.IsSuccessStatusCode)
                    {
                        if (SelectedUSIncome != null)
                        {
                            SelectedUSIncome.IncCode = IncCode;
                            SelectedUSIncome.IncDetail = IncDetail;
                            SelectedUSIncome.UID = UID;
                            SelectedUSIncome.EntryDate = EntryDate;
                        }
                        MessageBox.Show("Data updated successfully via API!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to update data. Status Code: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Creation and insertion logic for brand new records using HTTP POST
                    HttpResponseMessage response = await _httpClient.PostAsJsonAsync(ApiUrl, inputData);

                    if (response.IsSuccessStatusCode)
                    {
                        USIncomeList.Add(inputData);
                        MessageBox.Show("Data saved successfully via API!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to save data. Status Code: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                ClearFields();
                _isEditMode = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while communicating with the API: {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Save command is only allowed if fields pass fundamental validations
        private bool CanExecuteSave(object parameter)
        {
            return !string.IsNullOrWhiteSpace(IncCode) && IncCode.Length <= 2;
        }

        private void ExecuteEdit(object parameter)
        {
            if (parameter is AccUSIncomeHeadModel selectedModel)
            {
                _isEditMode = true;
                SelectedUSIncome = selectedModel;
            }
        }

        // Edit command can only run if an active row from the DataGrid grid system is selected
        private bool CanExecuteEdit(object parameter)
        {
            return parameter != null;
        }

        private void ExecuteExit(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                currentWindow.Close();
            }
        }

        #endregion

        #region Helper Methods

        private void ClearFields()
        {
            IncCode = string.Empty;
            IncDetail = string.Empty;
            UID = _currentUID; // Keep the active UID loaded in the input field layout structure
            EntryDate = null;
            SelectedUSIncome = null;
        }

        private async void LoadDataGridFromDatabase()
        {
            try
            {
                // Fetch data from the running API setup using HTTP GET
                var items = await _httpClient.GetFromJsonAsync<ObservableCollection<AccUSIncomeHeadModel>>(ApiUrl);
                if (items != null)
                {
                    USIncomeList.Clear();
                    foreach (var item in items)
                    {
                        USIncomeList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load data from API: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}