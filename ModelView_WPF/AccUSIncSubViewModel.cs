using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Model_WPF; // For AccUSIncSubModel and other models

namespace ModelView_WPF
{
    public class AccUSIncSubViewModel : INotifyPropertyChanged
    {
        // Private Fields
        private string _incSubCode = string.Empty;
        private string _incDetail = string.Empty;
        private string _incCode = string.Empty;
        private string _uid = string.Empty;
        private DateTime? _entryDate = DateTime.Now;
        private AccUSIncSubModel? _selectedUSIncSub;
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://74.208.227.170:8050//api/AccUSIncSub";

        // Collections for DataGrid and ComboBox
        public ObservableCollection<AccUSIncSubModel> USIncSubList { get; set; }
        public ObservableCollection<string> IncCodeList { get; set; }

        // ICommand Properties
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        // Default Constructor
        public AccUSIncSubViewModel()
        {
            // Initialize HttpClient Handler
            _httpClient = new HttpClient();

            // Collection Initialization
            USIncSubList = new ObservableCollection<AccUSIncSubModel>();
            IncCodeList = new ObservableCollection<string>();

            // Command Initialization using the separate RelayCommand class
            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            EditCommand = new RelayCommand(ExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            // Fetch and Bind Data asynchronously from the API
            LoadInitialData();
        }

        // Parameterized Constructor to receive UID from View
        public AccUSIncSubViewModel(string uid)
        {
            // Initialize HttpClient Handler
            _httpClient = new HttpClient();

            // Collection Initialization
            USIncSubList = new ObservableCollection<AccUSIncSubModel>();
            IncCodeList = new ObservableCollection<string>();

            // Command Initialization using the separate RelayCommand class
            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            EditCommand = new RelayCommand(ExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            // Assign the passed UID to the property
            UID = uid;

            // Fetch and Bind Data asynchronously from the API
            LoadInitialDataWithUID(uid);
        }

        #region Public Properties for View Binding
        public string IncSubCode
        {
            get => _incSubCode;
            set
            {
                if (_incSubCode != value)
                {
                    _incSubCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public string IncDetail
        {
            get => _incDetail;
            set
            {
                if (_incDetail != value)
                {
                    _incDetail = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedIncCode
        {
            get => _incCode;
            set
            {
                if (_incCode != value)
                {
                    _incCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public string UID
        {
            get => _uid;
            set
            {
                if (_uid != value)
                {
                    _uid = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? EntryDate
        {
            get => _entryDate;
            set
            {
                if (_entryDate != value)
                {
                    _entryDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public AccUSIncSubModel? SelectedUSIncSub
        {
            get => _selectedUSIncSub;
            set
            {
                if (_selectedUSIncSub != value)
                {
                    _selectedUSIncSub = value;
                    OnPropertyChanged();

                    // Map grid selection values into UI fields
                    if (_selectedUSIncSub != null)
                    {
                        IncSubCode = _selectedUSIncSub.IncSubCode ?? string.Empty;
                        SelectedIncCode = _selectedUSIncSub.IncCode ?? string.Empty;
                        IncDetail = _selectedUSIncSub.IncDetail ?? string.Empty;
                        UID = _selectedUSIncSub.UID ?? string.Empty;
                        EntryDate = _selectedUSIncSub.EntryDate;
                    }
                }
            }
        }
        #endregion

        #region Command Methods Execution
        private void ExecuteAdd(object? parameter)
        {
            // Reset entry fields for fresh record creation
            IncSubCode = string.Empty;
            IncDetail = string.Empty;
            SelectedIncCode = IncCodeList.Count > 0 ? IncCodeList[0] : string.Empty;
            UID = "01";
            EntryDate = DateTime.Now;
            SelectedUSIncSub = null;
        }

        private async void ExecuteSave(object? parameter)
        {
            var targetSubHead = new AccUSIncSubModel
            {
                IncSubCode = this.IncSubCode,
                IncCode = this.SelectedIncCode,
                IncDetail = this.IncDetail,
                UID = this.UID,
                EntryDate = this.EntryDate
            };

            try
            {
                HttpResponseMessage response;

                // Determine whether to perform POST (New) or PUT (Update) based on SelectedUSIncSub context
                if (SelectedUSIncSub == null)
                {
                    // POST Request to API
                    response = await _httpClient.PostAsJsonAsync(ApiBaseUrl, targetSubHead);
                }
                else
                {
                    // PUT Request to API
                    response = await _httpClient.PutAsJsonAsync($"{ApiBaseUrl}/{targetSubHead.IncSubCode}", targetSubHead);
                }

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Data Sync with Server Successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await RefreshGridData();
                    ExecuteAdd(null);
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Server Status Error: {response.StatusCode}\nDetails: {errorMsg}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection to API Failed: {ex.Message}", "Network Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteSave(object? parameter)
        {
            // Client validation rule check
            return !string.IsNullOrWhiteSpace(IncSubCode) && !string.IsNullOrWhiteSpace(IncDetail);
        }

        private void ExecuteEdit(object? parameter)
        {
            if (parameter is AccUSIncSubModel selectedItem)
            {
                MessageBox.Show($"Editing Mode Loaded for Target Sub Code: {selectedItem.IncSubCode}", "Edit Active", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }

        private void ExecuteExit(object? parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }
        #endregion

        #region Data Loading Helpers (API Integration)
        private async void LoadInitialData()
        {
            UID = "01"; // Setup baseline default user identifier
            await FetchComboBoxCodes();
            await RefreshGridData();
        }

        private async void LoadInitialDataWithUID(string uid)
        {
            await FetchComboBoxCodes();
            await RefreshGridData();
            // Ensure UID stays as the passed value after async loads
            UID = uid;
        }

        private async Task FetchComboBoxCodes()
        {
            try
            {
                // Call dedicated Controller Endpoint to fetch IncCode list from AccUSIncomeHead table
                var rawCodes = await _httpClient.GetFromJsonAsync<List<string>>($"{ApiBaseUrl}/GetIncCodes");
                if (rawCodes != null)
                {
                    IncCodeList.Clear();
                    foreach (var code in rawCodes)
                    {
                        IncCodeList.Add(code);
                    }

                    if (IncCodeList.Count > 0)
                    {
                        SelectedIncCode = IncCodeList[0];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to fetch target ComboBox records from API: {ex.Message}", "Metadata Exception", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Fallback local schema assignments if API target is unreachable
                IncCodeList.Clear();
                IncCodeList.Add("01");
                IncCodeList.Add("02");
                IncCodeList.Add("03");
            }
        }

        private async Task RefreshGridData()
        {
            try
            {
                // Fetch full existing table records from API
                var items = await _httpClient.GetFromJsonAsync<List<AccUSIncSubModel>>(ApiBaseUrl);
                if (items != null)
                {
                    USIncSubList.Clear();
                    foreach (var record in items)
                    {
                        USIncSubList.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to sync and refresh datagrid elements: {ex.Message}", "Fetch Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}