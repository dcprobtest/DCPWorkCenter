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
    // Inline Model class definition inside the same file to avoid extra files
    public class UserDropdownModel
    {
        public string UID { get; set; }
        public string DisplayText { get; set; }
    }

    public class AssignmentScrumViewModel : INotifyPropertyChanged
    {
        private AssignmentScrumModel _scrum;
        private AssignmentScrumModel _selectedScrum;
        private bool _isActivated;
        private bool _isLoading = false;

        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        // Collections for ComboBox data binding
        public ObservableCollection<int> AvailableAssignIDs { get; set; }
        public ObservableCollection<string> AvailableScrumIDs { get; set; }
        public ObservableCollection<UserDropdownModel> AvailableUsers { get; set; }

        public AssignmentScrumModel Scrum
        {
            get => _scrum;
            set { _scrum = value; OnPropertyChanged(); }
        }

        public ObservableCollection<AssignmentScrumModel> ScrumList { get; set; }

        public AssignmentScrumModel SelectedScrum
        {
            get => _selectedScrum;
            set
            {
                if (_selectedScrum != value)
                {
                    _selectedScrum = value;
                    OnPropertyChanged();

                    if (_isLoading) return;

                    if (_selectedScrum != null)
                    {
                        Scrum = new AssignmentScrumModel
                        {
                            AssignID = _selectedScrum.AssignID,
                            ScrumID = _selectedScrum.ScrumID,
                            AssignByUID = _selectedScrum.AssignByUID,
                            AssignForUID = _selectedScrum.AssignForUID,
                            StartStatus = _selectedScrum.StartStatus,
                            StartDate = _selectedScrum.StartDate,
                            ProgressStatus = _selectedScrum.ProgressStatus,
                            ProgressDate = _selectedScrum.ProgressDate,
                            CompleteStatus = _selectedScrum.CompleteStatus,
                            CompleteDate = _selectedScrum.CompleteDate,
                            AssignByDesc = _selectedScrum.AssignByDesc,
                            AssignForDesc = _selectedScrum.AssignForDesc,
                            Activate = _selectedScrum.Activate
                        };
                        IsActivated = (_scrum.Activate == "Y");
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
                    if (Scrum != null)
                    {
                        Scrum.Activate = _isActivated ? "Y" : "N";
                    }
                }
            }
        }

        public ICommand AddCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        public AssignmentScrumViewModel()
        {
            Scrum = new AssignmentScrumModel { StartDate = DateTime.Now, Activate = "N" };
            ScrumList = new ObservableCollection<AssignmentScrumModel>();

            // Initialize collections
            AvailableAssignIDs = new ObservableCollection<int>();
            AvailableScrumIDs = new ObservableCollection<string>();
            AvailableUsers = new ObservableCollection<UserDropdownModel>();

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(ExecuteSave);
            EditCommand = new RelayCommand(ExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            LoadDataFromApi();
            LoadUsersFromApi();
        }

        // New overloaded constructor to accept UID from code-behind and bind it to AssignByUID
        public AssignmentScrumViewModel(string uid) : this()
        {
            if (Scrum != null)
            {
                Scrum.AssignByUID = uid;
            }
        }

        public AssignmentScrumViewModel(int assignId, string scrumId) : this()
        {
            Scrum.AssignID = assignId;
            Scrum.ScrumID = scrumId;
        }

        private void AppReset()
        {
            // Keep the logged-in UID intact even after reset if needed, or clear based on requirement
            string currentUID = Scrum?.AssignByUID ?? string.Empty;

            Scrum = new AssignmentScrumModel
            {
                AssignID = 0,
                ScrumID = string.Empty,
                AssignByUID = currentUID,
                AssignForUID = string.Empty,
                StartStatus = string.Empty,
                StartDate = DateTime.Now,
                ProgressStatus = string.Empty,
                ProgressDate = null,
                CompleteStatus = string.Empty,
                CompleteDate = null,
                AssignByDesc = string.Empty,
                AssignForDesc = string.Empty,
                Activate = "N"
            };
            IsActivated = false;
            SelectedScrum = null;
        }

        private void ExecuteAdd(object parameter)
        {
            AppReset();
        }

        private async void LoadDataFromApi()
        {
            if (_isLoading) return;
            try
            {
                _isLoading = true;

                // 1. Load AssignmentScrum data for the main grid/list using ApiSettings
                var response = await _httpClient.GetAsync(ApiSettings.AssignmentScrumBaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<List<AssignmentScrumModel>>(jsonString, _jsonOptions);

                    if (data != null)
                    {
                        ScrumList.Clear();
                        foreach (var item in data)
                        {
                            ScrumList.Add(item);
                        }
                    }
                }

                // 2. Load AssignID dropdown data from API endpoint using ApiSettings
                var assignResponse = await _httpClient.GetAsync(ApiSettings.GetAssignmentScrumAvailableAssignIDsUrl());
                if (assignResponse.IsSuccessStatusCode)
                {
                    string assignJson = await assignResponse.Content.ReadAsStringAsync();
                    var assignIds = JsonSerializer.Deserialize<List<int>>(assignJson, _jsonOptions);
                    if (assignIds != null)
                    {
                        AvailableAssignIDs.Clear();
                        foreach (var id in assignIds)
                        {
                            AvailableAssignIDs.Add(id);
                        }
                    }
                }

                // 3. Load ScrumID dropdown data from API endpoint using ApiSettings
                var scrumResponse = await _httpClient.GetAsync(ApiSettings.GetAssignmentScrumAvailableScrumIDsUrl());
                if (scrumResponse.IsSuccessStatusCode)
                {
                    string scrumJson = await scrumResponse.Content.ReadAsStringAsync();
                    var scrumIds = JsonSerializer.Deserialize<List<string>>(scrumJson, _jsonOptions);
                    if (scrumIds != null)
                    {
                        AvailableScrumIDs.Clear();
                        foreach (var id in scrumIds)
                        {
                            AvailableScrumIDs.Add(id);
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

        private async void LoadUsersFromApi()
        {
            try
            {
                var response = await _httpClient.GetAsync(ApiSettings.UserDropdownUrl);
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    var users = JsonSerializer.Deserialize<List<UserDropdownModel>>(jsonString, _jsonOptions);
                    if (users != null)
                    {
                        AvailableUsers.Clear();
                        foreach (var user in users)
                        {
                            AvailableUsers.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ExecuteSave(object parameter)
        {
            try
            {
                if (Scrum.AssignID <= 0 || string.IsNullOrWhiteSpace(Scrum.ScrumID))
                {
                    MessageBox.Show("Please fill required fields (Assign ID and Scrum ID).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var response = await _httpClient.PostAsJsonAsync(ApiSettings.AssignmentScrumBaseUrl, Scrum);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Data saved successfully via API.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDataFromApi();
                    AppReset();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to save. API Error: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (Scrum.AssignID <= 0 || string.IsNullOrWhiteSpace(Scrum.ScrumID))
                {
                    MessageBox.Show("Please select a valid record with Assign ID and Scrum ID to update.", "Selection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var response = await _httpClient.PutAsJsonAsync(ApiSettings.AssignmentScrumUpdateStatusUrl, Scrum);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Data updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadDataFromApi();
                    AppReset();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to update. API Error: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteExit(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
            else
            {
                foreach (Window win in Application.Current.Windows)
                {
                    if (win.DataContext == this)
                    {
                        win.Close();
                        return;
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}