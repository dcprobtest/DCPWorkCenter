using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using Model_WPF; // Adjust namespace if your TechInfoModel is located elsewhere
using ModelView_WPF; // Added to reference the RelayCommand namespace and fix compilation errors

namespace View_WPF.ModelView_WPF
{
    public class TechInfoViewModel : INotifyPropertyChanged
    {
        private TechInfoModel _tech;
        private TechInfoModel _selectedTech;
        private ObservableCollection<TechInfoModel> _techList;
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://localhost:7073/api/TechInfo"; // Configured from Swagger URL Context

        // Global variable to store the logged-in UID passed from the view
        private string _passedUid = "USR001";

        // Properties bound to the XAML View
        public TechInfoModel Tech
        {
            get => _tech;
            set
            {
                _tech = value;
                OnPropertyChanged();
            }
        }

        public TechInfoModel SelectedTech
        {
            get => _selectedTech;
            set
            {
                _selectedTech = value;
                OnPropertyChanged();
                if (_selectedTech != null)
                {
                    // Map selected list item data to input form fields
                    Tech = new TechInfoModel
                    {
                        TechId = _selectedTech.TechId,
                        TechDesc = _selectedTech.TechDesc,
                        StartDate = _selectedTech.StartDate,
                        EntryDate = _selectedTech.EntryDate,
                        EndDate = _selectedTech.EndDate,
                        EntryByUid = _selectedTech.EntryByUid,
                        LastUpdate = _selectedTech.LastUpdate,
                        Activate = _selectedTech.Activate
                    };
                }
            }
        }

        public ObservableCollection<TechInfoModel> TechList
        {
            get => _techList;
            set
            {
                _techList = value;
                OnPropertyChanged();
            }
        }

        // ICommand declarations bound to XAML buttons
        public ICommand AddCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        public TechInfoViewModel()
        {
            // Initialize HttpClient for API pipeline communication
            _httpClient = new HttpClient();

            // Initialize working instances
            Tech = new TechInfoModel();
            TechList = new ObservableCollection<TechInfoModel>();

            // Setup default user profile data if necessary
            Tech.EntryByUid = _passedUid;
            Tech.EntryDate = DateTime.Now;

            // Initialize form operations mapping to your RelayCommand class
            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            // Load live state data from API instead of local mock data
            LoadApiData();
        }

        // Overloaded constructor to receive the Uid from the View
        public TechInfoViewModel(string uid)
        {
            if (!string.IsNullOrEmpty(uid))
            {
                _passedUid = uid;
            }

            // Initialize HttpClient for API pipeline communication
            _httpClient = new HttpClient();

            // Initialize working instances
            Tech = new TechInfoModel();
            TechList = new ObservableCollection<TechInfoModel>();

            // Setup default user profile data if necessary
            Tech.EntryByUid = _passedUid;
            Tech.EntryDate = DateTime.Now;

            // Initialize form operations mapping to your RelayCommand class
            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            // Load live state data from API instead of local mock data
            LoadApiData();
        }

        // Action operations logic
        private void ExecuteAdd(object parameter)
        {
            Tech = new TechInfoModel
            {
                EntryByUid = _passedUid,
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now
            };
            SelectedTech = null;
        }

        private async void ExecuteSave(object parameter)
        {
            try
            {
                Tech.LastUpdate = DateTime.Now;

                // Anonymously map to payload aligning with database and API entity contract
                var payload = new
                {
                    techID = Tech.TechId,
                    techDesc = Tech.TechDesc,
                    startDate = Tech.StartDate,
                    entryDate = Tech.EntryDate,
                    endDate = Tech.EndDate,
                    entryByUID = Tech.EntryByUid,
                    lastUpdate = Tech.LastUpdate,
                    activate = Tech.Activate ?? "Y"
                };

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync(ApiUrl, payload);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Technology data successfully saved to the server database.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadApiData(); // Refresh grid state directly from database persistence
                    ExecuteAdd(null);
                }
                else
                {
                    MessageBox.Show($"Failed to save data. API Server returned: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API Pipeline connection exception: {ex.Message}", "Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteSave(object parameter)
        {
            // Validation rule: TechId is mandatory column structure requirement
            return Tech != null && !string.IsNullOrWhiteSpace(Tech.TechId);
        }

        private async void ExecuteEdit(object parameter)
        {
            if (SelectedTech != null)
            {
                try
                {
                    Tech.LastUpdate = DateTime.Now;

                    var payload = new
                    {
                        techID = Tech.TechId,
                        techDesc = Tech.TechDesc,
                        startDate = Tech.StartDate,
                        entryDate = Tech.EntryDate,
                        endDate = Tech.EndDate,
                        entryByUID = Tech.EntryByUid,
                        lastUpdate = Tech.LastUpdate,
                        activate = Tech.Activate ?? "Y"
                    };

                    string putUrl = $"{ApiUrl}/{Tech.TechId}";
                    HttpResponseMessage response = await _httpClient.PutAsJsonAsync(putUrl, payload);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Server data persistence context update complete.", "Update", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadApiData(); // Re-sync local UI thread collection state
                        ExecuteAdd(null);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to update records. API Server returned: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"API Pipeline connection exception: {ex.Message}", "Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanExecuteEdit(object parameter)
        {
            return SelectedTech != null;
        }

        private void ExecuteExit(object parameter)
        {
            Application.Current.MainWindow?.Close();
        }

        private async void LoadApiData()
        {
            try
            {
                // Fetch dynamic array matching backend structure representation safely
                var apiRecords = await _httpClient.GetFromJsonAsync<System.Collections.Generic.List<dynamic>>(ApiUrl);

                TechList.Clear();
                if (apiRecords != null)
                {
                    foreach (var record in apiRecords)
                    {
                        TechList.Add(new TechInfoModel
                        {
                            TechId = record.GetProperty("techID").GetString(),
                            TechDesc = record.GetProperty("techDesc").GetString(),
                            StartDate = record.GetProperty("startDate").GetDateTime(),
                            EntryDate = record.GetProperty("entryDate").GetDateTime(),

                            // Fixed: Explicit out parameter types provided to resolve dynamic inference constraints (CS8197)
                            EndDate = record.TryGetProperty("endDate", out System.Text.Json.JsonElement endVal) && endVal.ValueKind != System.Text.Json.JsonValueKind.Null ? endVal.GetDateTime() : (DateTime?)null,

                            EntryByUid = record.GetProperty("entryByUID").GetString(),

                            // Fixed: Explicit out parameter types provided to resolve dynamic inference constraints (CS8197)
                            LastUpdate = record.TryGetProperty("lastUpdate", out System.Text.Json.JsonElement upVal) && upVal.ValueKind != System.Text.Json.JsonValueKind.Null ? upVal.GetDateTime() : (DateTime?)null,

                            Activate = record.GetProperty("activate").GetString()
                        });
                    }
                }
            }
            catch (Exception)
            {
                // Fallback architecture maintaining design-time WPF structural verification
                LoadMockData();
            }
        }

        private void LoadMockData()
        {
            // Mock records safely illustrating initial grid population mechanics
            TechList.Clear();
            TechList.Add(new TechInfoModel { TechId = "001", TechDesc = "WPF Desktop Engineering Architecture", Activate = "Y", EntryByUid = "SYSADMIN", EntryDate = DateTime.Now });
            TechList.Add(new TechInfoModel { TechId = "002", TechDesc = "ASP.NET Core Enterprise Web API Services", Activate = "Y", EntryByUid = "SYSADMIN", EntryDate = DateTime.Now });
            TechList.Add(new TechInfoModel { TechId = "003", TechDesc = "SQL Server High-Performance Relational Schema", Activate = "N", EntryByUid = "SYSADMIN", EntryDate = DateTime.Now });
        }

        // Property tracking plumbing definition
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}