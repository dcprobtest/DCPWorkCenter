using Model_WPF;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;

namespace ModelView_WPF
{
    public class ScrumInfoViewModel : INotifyPropertyChanged
    {
        private ScrumInfoModel _scrum;
        private ScrumInfoModel _selectedScrum;
        private ObservableCollection<ScrumInfoModel> _scrumList;
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://localhost:7073/api/ScrumInfo";

        // SignalR client connection global variable
        private HubConnection _hubConnection;

        // Properties bound to the View
        public ScrumInfoModel Scrum
        {
            get => _scrum;
            set
            {
                _scrum = value;
                OnPropertyChanged();
            }
        }

        public ScrumInfoModel SelectedScrum
        {
            get => _selectedScrum;
            set
            {
                _selectedScrum = value;
                OnPropertyChanged();
                if (_selectedScrum != null)
                {
                    // Map selected list item to form inputs
                    LoadSelectedScrum(_selectedScrum);
                }
            }
        }

        public ObservableCollection<ScrumInfoModel> ScrumList
        {
            get => _scrumList;
            set
            {
                _scrumList = value;
                OnPropertyChanged();
            }
        }

        // RelayCommands for Buttons
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        public ScrumInfoViewModel()
        {
            // Initialize HttpClient for API connection
            _httpClient = new HttpClient();

            // Initialize model and collection
            Scrum = new ScrumInfoModel();
            ScrumList = new ObservableCollection<ScrumInfoModel>();

            // Initialize commands using RelayCommand from your project
            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            // Default initial values
            ResetForm();

            // Load data asynchronously from API on startup
            _ = LoadDataFromApiAsync();

            // Initialize and start SignalR live connection setup
            InitializeSignalR();
        }

        // Initialize SignalR and setup listeners
        public async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7073/scrumHub")
                .WithAutomaticReconnect()
                .Build();

            // When API broadcasts "RefreshScrumList", WPF executes this logic
            _hubConnection.On("RefreshScrumList", () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                {
                    // Triggers the existing API fetch method to reload data instantly
                    await LoadDataFromApiAsync();
                });
            });

            try
            {
                // Start connection
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                // Soft log if connection fails initially, avoids crashing UI
                System.Diagnostics.Debug.WriteLine($"SignalR Connection Error: {ex.Message}");
            }
        }

        // Fetch Data from API
        private async Task LoadDataFromApiAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ObservableCollection<ScrumInfoModel>>(ApiUrl);
                if (response != null)
                {
                    ScrumList = response;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data from API: {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Business Logic Methods
        private void ExecuteAdd(object parameter)
        {
            ResetForm();
        }

        private async void ExecuteSave(object parameter)
        {
            // Logic to save new record to database via API
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiUrl, Scrum);
                if (response.IsSuccessStatusCode)
                {
                    ScrumList.Add(new ScrumInfoModel
                    {
                        ScrumID = Scrum.ScrumID,
                        ScrumDesc = Scrum.ScrumDesc,
                        EntryDate = Scrum.EntryDate,
                        StartDate = Scrum.StartDate,
                        EndDate = Scrum.EndDate,
                        EntryByUID = Scrum.EntryByUID,
                        LastUpdate = Scrum.LastUpdate,
                        Activate = Scrum.Activate
                    });

                    ResetForm();
                }
                else
                {
                    MessageBox.Show($"Failed to save data. Status Code: {response.StatusCode}", "API Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to API: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteSave(object parameter)
        {
            // Validation: ScrumID is required (Unchecked/NotNull in database)
            return !string.IsNullOrWhiteSpace(Scrum?.ScrumID);
        }

        private async void ExecuteEdit(object parameter)
        {
            // Logic to update existing selected record via API
            if (SelectedScrum != null)
            {
                try
                {
                    string putUrl = $"{ApiUrl}/{Scrum.ScrumID}";
                    var response = await _httpClient.PutAsJsonAsync(putUrl, Scrum);

                    if (response.IsSuccessStatusCode)
                    {
                        SelectedScrum.ScrumID = Scrum.ScrumID;
                        SelectedScrum.ScrumDesc = Scrum.ScrumDesc;
                        SelectedScrum.EntryDate = Scrum.EntryDate;
                        SelectedScrum.StartDate = Scrum.StartDate;
                        SelectedScrum.EndDate = Scrum.EndDate;
                        SelectedScrum.EntryByUID = Scrum.EntryByUID;
                        SelectedScrum.LastUpdate = Scrum.LastUpdate;
                        SelectedScrum.Activate = Scrum.Activate;
                    }
                    else
                    {
                        MessageBox.Show($"Failed to update data. Status Code: {response.StatusCode}", "API Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error connecting to API: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool CanExecuteEdit(object parameter)
        {
            return SelectedScrum != null;
        }

        private void ExecuteExit(object parameter)
        {
            // Logic to close window or system application
            System.Windows.Application.Current.Shutdown();
        }

        private void LoadSelectedScrum(ScrumInfoModel source)
        {
            Scrum.ScrumID = source.ScrumID;
            Scrum.ScrumDesc = source.ScrumDesc;
            Scrum.EntryDate = source.EntryDate;
            Scrum.StartDate = source.StartDate;
            Scrum.EndDate = source.EndDate;
            Scrum.EntryByUID = source.EntryByUID;
            Scrum.LastUpdate = source.LastUpdate;
            Scrum.Activate = source.Activate;
        }

        private void ResetForm()
        {
            Scrum.ScrumID = string.Empty;
            Scrum.ScrumDesc = string.Empty;
            Scrum.EntryDate = DateTime.Now;
            Scrum.StartDate = DateTime.Now;
            Scrum.EndDate = DateTime.Now.AddDays(30);
            Scrum.EntryByUID = "ADMIN"; // Read-only default placeholder
            Scrum.LastUpdate = DateTime.Now;
            Scrum.Activate = "Y";
        }

        // INotifyPropertyChanged Implementation
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}