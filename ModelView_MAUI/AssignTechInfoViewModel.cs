using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace View_MAUI
{
    // 1. Separate Data Model Class
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
        private DateTime _entryDate; // MAUI DatePicker requires non-nullable DateTime
        private string _activate;

        private bool _isLoading = false;

        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://localhost:7073/api/AssignTechInfo";
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

        public DateTime EntryDate
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

        public ObservableCollection<AssignTechInfoModel> TechInfoList { get; set; }
        public ObservableCollection<string> ScrumList { get; set; }

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
                            this.EntryDate = _selectedTechInfo.EntryDate ?? DateTime.Now;
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

        public Command AddCommand { get; private set; }
        public Command SaveCommand { get; private set; }
        public Command EditCommand { get; private set; }
        public Command ExitCommand { get; private set; }

        // Default Constructor
        public AssignTechInfoViewModel()
        {
            TechInfoList = new ObservableCollection<AssignTechInfoModel>();
            ScrumList = new ObservableCollection<string>();

            // Bypass self-signed SSL local certificate policy for MAUI Cross-Platform development
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            this.EntryDate = DateTime.Now;
            this.Activate = "N";

            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async () => await ExecuteSave());
            EditCommand = new Command(async () => await ExecuteEdit());
            ExitCommand = new Command(async () => await ExecuteExit());

            _ = LoadDataFromApi();
        }

        public AssignTechInfoViewModel(string uid) : this()
        {
            this.EntryByUID = uid;
        }

        private void ExecuteAdd()
        {
            _isLoading = true;
            try
            {
                this.AssignTechInfoID = 0;
                this.AssignTechInfoIDFormatted = string.Empty;
                this.AssignID = null;
                this.ScrumID = null;
                this.EntryByUID = string.Empty;
                this.EntryDate = DateTime.Now;
                this.IsActivated = false;
                this.SelectedTechInfo = null;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task LoadDataFromApi()
        {
            if (_isLoading) return;

            try
            {
                _isLoading = true;

                var response = await _httpClient.GetAsync(ApiUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
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
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("API Error", $"Error loading data from API: {ex.Message}", "OK");
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task ExecuteSave()
        {
            try
            {
                if (this.AssignID == null || this.AssignID == 0 || string.IsNullOrWhiteSpace(this.ScrumID))
                {
                    await Application.Current.MainPage.DisplayAlert("Validation Error", "Please fill required fields (Assign ID and Scrum ID).\nMake sure Scrum ID is selected properly.", "OK");
                    return;
                }

                string finalScrumID = this.ScrumID.Trim();
                if (finalScrumID.Length > 5)
                {
                    finalScrumID = finalScrumID.Substring(0, 5);
                }

                var payload = new
                {
                    assignTechInfoID = 0,
                    assignID = this.AssignID,
                    scrumID = finalScrumID,
                    entryByUID = string.IsNullOrWhiteSpace(this.EntryByUID) ? null : this.EntryByUID.Trim(),
                    entryDate = this.EntryDate,
                    activate = this.Activate
                };

                var response = await _httpClient.PostAsJsonAsync(ApiUrl, payload);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Notification", "Data saved successfully via API.", "OK");
                    await LoadDataFromApi();
                    ExecuteAdd();
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("API Error", $"Failed to save data. API Response: {errorMsg}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task ExecuteEdit()
        {
            try
            {
                if (this.AssignTechInfoID == 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Selection Error", "Please select a valid record from the list to update.", "OK");
                    return;
                }

                var payload = new
                {
                    assignTechInfoID = this.AssignTechInfoID,
                    assignID = this.AssignID,
                    scrumID = this.ScrumID,
                    entryByUID = this.EntryByUID,
                    entryDate = this.EntryDate,
                    activate = this.Activate
                };

                string updateUrl = $"{ApiUrl}/{this.AssignTechInfoID}";
                var response = await _httpClient.PutAsJsonAsync(updateUrl, payload);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Notification", "Data updated successfully.", "OK");
                    await LoadDataFromApi();
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("API Error", $"Failed to update data. API Response: {errorMsg}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred during update: {ex.Message}", "OK");
            }
        }

        private async Task ExecuteExit()
        {
            if (Application.Current?.MainPage?.Navigation != null)
            {
                // Pure MVVM safe navigation back pop
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}