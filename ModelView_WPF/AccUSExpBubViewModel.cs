using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Model_WPF; // References the Model project

namespace ModelView_WPF
{
    public class AccUSExpBubViewModel : INotifyPropertyChanged
    {
        // Property Changed Event for UI Data Binding Notification
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // HttpClient instance for API Communication
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://74.208.227.170:8050//") };

        // Privates for Form Visibility and States
        private bool _isEntryVisible;
        private AccUSExpBubModel _currentEntry;
        private ObservableCollection<AccUSExpBubModel> _expBubList;
        private ObservableCollection<string> _expCodeList; // Keeping simple list for dropdown strings as requested
        private string _selectedExpCode;

        // Private variable to hold the passed logged-in User ID
        private string _loggedInUID = string.Empty;

        // Public Properties for Binding with frmAccUSExpBub.xaml
        public bool IsEntryVisible
        {
            get => _isEntryVisible;
            set { _isEntryVisible = value; OnPropertyChanged(); }
        }

        public AccUSExpBubModel CurrentEntry
        {
            get => _currentEntry;
            set { _currentEntry = value; OnPropertyChanged(); }
        }

        public ObservableCollection<AccUSExpBubModel> ExpBubList
        {
            get => _expBubList;
            set { _expBubList = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> ExpCodeList
        {
            get => _expCodeList;
            set { _expCodeList = value; OnPropertyChanged(); }
        }

        public string SelectedExpCode
        {
            get => _selectedExpCode;
            set
            {
                _selectedExpCode = value;
                OnPropertyChanged();
                if (CurrentEntry != null)
                {
                    CurrentEntry.ExpCode = value;
                }
            }
        }

        // ICommands mapped to your XAML Buttons
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        // Default Constructor
        public AccUSExpBubViewModel() : this(string.Empty)
        {
        }

        // Parameterized Constructor to receive UID from the View
        public AccUSExpBubViewModel(string uid)
        {
            // Store the received UID
            _loggedInUID = uid;

            // Initialize collections and entities
            ExpBubList = new ObservableCollection<AccUSExpBubModel>();
            ExpCodeList = new ObservableCollection<string>();
            CurrentEntry = new AccUSExpBubModel();

            // Default Form States
            IsEntryVisible = false;

            // Link commands with methods using your project's RelayCommand
            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            // Load data initial states from API
            LoadGridData();
            LoadDropdowns();
        }

        // Methods to pull lists from database repository/service layers via API
        private async void LoadGridData()
        {
            try
            {
                var records = await _httpClient.GetFromJsonAsync<List<AccUSExpBubModel>>("api/AccUSExpBub");
                if (records != null)
                {
                    ExpBubList.Clear();
                    foreach (var item in records)
                    {
                        ExpBubList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading grid data: {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadDropdowns()
        {
            try
            {
                // Fetches data from tblAccUSExpendHead controller endpoint
                var expHeads = await _httpClient.GetFromJsonAsync<List<AccUSExpendHeadModel>>("api/AccUSExpBub/GetExpCodes");
                if (expHeads != null)
                {
                    ExpCodeList.Clear();
                    foreach (var head in expHeads)
                    {
                        if (!string.IsNullOrEmpty(head.ExpCode))
                        {
                            ExpCodeList.Add(head.ExpCode); // Populating the ComboBox items string collection
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dropdowns: {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Triggered when ADD button is pressed
        private void ExecuteAdd(object parameter)
        {
            CurrentEntry = new AccUSExpBubModel
            {
                EntryDate = DateTime.Now,
                UID = _loggedInUID // Assigns the actual logged-in user id dynamically instead of hardcoded "U001"
            };
            SelectedExpCode = null; // Clear old dropdown states
            IsEntryVisible = true;
        }

        // Triggered when SAVE button is pressed
        private async void ExecuteSave(object parameter)
        {
            try
            {
                // API call to insert or update CurrentEntry inside database table tblUSExpBub
                var response = await _httpClient.PostAsJsonAsync("api/AccUSExpBub", CurrentEntry);

                if (response.IsSuccessStatusCode)
                {
                    // Refresh list and clear selection panel
                    LoadGridData();
                    IsEntryVisible = false;
                    CurrentEntry = new AccUSExpBubModel();
                    SelectedExpCode = null;
                }
                else
                {
                    MessageBox.Show("Failed to save data to server.", "API Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing save operation: {ex.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteSave(object parameter)
        {
            // Validation criteria logic before hitting database save operation
            return CurrentEntry != null && !string.IsNullOrEmpty(CurrentEntry.ExpCode);
        }

        // Triggered when EDIT button is pressed
        private void ExecuteEdit(object parameter)
        {
            if (parameter is AccUSExpBubModel selectedItem)
            {
                CurrentEntry = selectedItem;
                SelectedExpCode = selectedItem.ExpCode;
                IsEntryVisible = true;
            }
        }

        private bool CanExecuteEdit(object parameter)
        {
            return true; // Controls when the Edit operation can run
        }

        // Triggered when EXIT button is pressed
        private void ExecuteExit(object parameter)
        {
            // Close form logic or clear current panel visualization
            IsEntryVisible = false;
        }
    }
}