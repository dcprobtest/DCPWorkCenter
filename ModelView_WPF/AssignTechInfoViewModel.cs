using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using Model_WPF;
using ModelView_WPF.Helpers;

namespace ModelView_WPF
{
    // 1. Separate Data Model Class (To permanently prevent WPF Rendering Loop and API Spamming)
    public class AssignTechInfoModel
    {
        public int AssignTechInfoID { get; set; }
        public string AssignTechInfoIDFormatted { get; set; }
        public int? AssignID { get; set; }
        public string ScrumID { get; set; }
        public string EntryByUID { get; set; }
        public DateTime? EntryDate { get; set; }
        public string Activate { get; set; }
    }

    // 2. Main ViewModel Class
    public class AssignTechInfoViewModel : INotifyPropertyChanged
    {
        private AssignTechInfoModel _selectedTechInfo;
        private bool _isActivated;

        private int _assignTechInfoID;
        private string _assignTechInfoIDFormatted;
        private int? _assignID;
        private string _scrumID;
        private string _entryByUID;
        private DateTime? _entryDate;
        private string _activate;

        private bool _isLoading = false;

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public int AssignTechInfoID
        {
            get => _assignTechInfoID;
            set { if (_assignTechInfoID != value) { _assignTechInfoID = value; OnPropertyChanged(); } }
        }

        public string AssignTechInfoIDFormatted
        {
            get => _assignTechInfoIDFormatted;
            set { if (_assignTechInfoIDFormatted != value) { _assignTechInfoIDFormatted = value; OnPropertyChanged(); } }
        }

        public int? AssignID
        {
            get => _assignID;
            set { if (_assignID != value) { _assignID = value; OnPropertyChanged(); } }
        }

        public string ScrumID
        {
            get => _scrumID;
            set { if (_scrumID != value) { _scrumID = value; OnPropertyChanged(); } }
        }

        public string EntryByUID
        {
            get => _entryByUID;
            set { if (_entryByUID != value) { _entryByUID = value; OnPropertyChanged(); } }
        }

        public DateTime? EntryDate
        {
            get => _entryDate;
            set { if (_entryDate != value) { _entryDate = value; OnPropertyChanged(); } }
        }

        public string Activate
        {
            get => _activate;
            set
            {
                if (_activate != value)
                {
                    _activate = value;
                    OnPropertyChanged();

                    // Sync IsActivated with back-field safety
                    bool newValue = (_activate == "Y" || _activate == "1" || _activate?.ToLower() == "y");
                    if (_isActivated != newValue)
                    {
                        _isActivated = newValue;
                        OnPropertyChanged(nameof(IsActivated));
                    }
                }
            }
        }

        public bool IsActivated
        {
            get => _isActivated;
            set
            {
                if (_isActivated != value)
                {
                    _isActivated = value;
                    OnPropertyChanged();
                    Activate = _isActivated ? "Y" : "N";
                }
            }
        }

        // Collection type updated to AssignTechInfoModel
        public ObservableCollection<AssignTechInfoModel> TechInfoList { get; set; }
        public ObservableCollection<string> ScrumList { get; set; }

        // Selected item type updated to AssignTechInfoModel
        public AssignTechInfoModel SelectedTechInfo
        {
            get => _selectedTechInfo;
            set
            {
                if (_selectedTechInfo != value)
                {
                    _selectedTechInfo = value;
                    OnPropertyChanged();

                    if (_isLoading) return;

                    if (_selectedTechInfo != null)
                    {
                        _isLoading = true;
                        try
                        {
                            this.AssignTechInfoID = _selectedTechInfo.AssignTechInfoID;
                            this.AssignTechInfoIDFormatted = _selectedTechInfo.AssignTechInfoIDFormatted;
                            this.AssignID = _selectedTechInfo.AssignID;
                            this.ScrumID = _selectedTechInfo.ScrumID;
                            this.EntryByUID = _selectedTechInfo.EntryByUID;
                            this.EntryDate = _selectedTechInfo.EntryDate;
                            this.Activate = _selectedTechInfo.Activate;
                        }
                        finally
                        {
                            _isLoading = false;
                        }
                    }
                }
            }
        }

        public ICommand AddCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        // Default Parameterless Constructor
        public AssignTechInfoViewModel()
        {
            TechInfoList = new ObservableCollection<AssignTechInfoModel>();
            ScrumList = new ObservableCollection<string>();

            // Localhost self-signed SSL certificate error bypass handler (Fixes: SSL Connection Could Not Be Established)
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Default initialization
            this.EntryDate = DateTime.Now;
            this.Activate = "N";

            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(ExecuteSave);
            EditCommand = new RelayCommand(ExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            LoadDataFromApi();
        }

        // New Overloaded Constructor to receive UID from Window Code-Behind
        public AssignTechInfoViewModel(string uid) : this()
        {
            this.EntryByUID = uid;
        }

        private void ExecuteAdd(object parameter)
        {
            _isLoading = true;
            try
            {
                this.AssignTechInfoID = 0;
                this.AssignTechInfoIDFormatted = string.Empty;
                this.AssignID = null;
                this.ScrumID = null; // Easy way to reset ComboBox
                this.EntryByUID = string.Empty;
                this.EntryDate = DateTime.Now;
                this.IsActivated = false;
                this.SelectedTechInfo = null; // To clear list selection
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async void LoadDataFromApi()
        {
            // If already running, secondary calls will not execute, which prevents endless API loops
            if (_isLoading) return;

            try
            {
                _isLoading = true;

                var response = await _httpClient.GetAsync(ApiSettings.AssignTechInfoBaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();

                    // Using the new Model class during deserialization
                    var data = JsonSerializer.Deserialize<List<AssignTechInfoModel>>(jsonString, _jsonOptions);

                    if (data != null)
                    {
                        TechInfoList.Clear();
                        ScrumList.Clear();

                        foreach (var item in data)
                        {
                            TechInfoList.Add(item);

                            if (!string.IsNullOrEmpty(item.ScrumID) && !ScrumList.Contains(item.ScrumID))
                            {
                                ScrumList.Add(item.ScrumID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data from API: {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async void ExecuteSave(object parameter)
        {
            try
            {
                // 1. Enhanced and safe trim validation check
                if (this.AssignID == null || this.AssignID == 0 || string.IsNullOrWhiteSpace(this.ScrumID))
                {
                    MessageBox.Show("Please fill required fields (Assign ID and Scrum ID).\nMake sure Scrum ID is selected properly.",
                                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Safety check to ensure Scrum ID does not exceed 5 characters as per database requirements
                string finalScrumID = this.ScrumID.Trim();
                if (finalScrumID.Length > 5)
                {
                    finalScrumID = finalScrumID.Substring(0, 5);
                }

                var payload = new
                {
                    assignTechInfoID = 0, // Always sending 0 since it is a new entry
                    assignID = this.AssignID,
                    scrumID = finalScrumID,
                    entryByUID = string.IsNullOrWhiteSpace(this.EntryByUID) ? null : this.EntryByUID.Trim(),
                    entryDate = this.EntryDate ?? DateTime.Now,
                    activate = this.Activate
                };

                var response = await _httpClient.PostAsJsonAsync(ApiSettings.AssignTechInfoBaseUrl, payload);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Data saved successfully via API.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Maintaining sequence to prevent API refresh loops
                    LoadDataFromApi();
                    ExecuteAdd(null);
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to save data. API Response: {errorMsg}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExecuteEdit(object parameter)
        {
            try
            {
                if (this.AssignTechInfoID == 0)
                {
                    MessageBox.Show("Please select a valid record from the list to update.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var payload = new
                {
                    assignTechInfoID = this.AssignTechInfoID,
                    assignID = this.AssignID,
                    scrumID = this.ScrumID,
                    entryByUID = this.EntryByUID,
                    entryDate = this.EntryDate ?? DateTime.Now,
                    activate = this.Activate
                };

                string updateUrl = ApiSettings.GetAssignTechInfoUpdateUrl(this.AssignTechInfoID);
                var response = await _httpClient.PutAsJsonAsync(updateUrl, payload);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Data updated successfully.", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDataFromApi();
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to update data. API Response: {errorMsg}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during update: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteExit(object parameter)
        {
            if (parameter is Window currentWindow)
            {
                currentWindow.Close();
            }
            else
            {
                Application.Current.MainWindow?.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}