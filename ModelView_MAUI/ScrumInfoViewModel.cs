using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.SignalR.Client;
using Model_MAUI;

namespace ModelView_MAUI
{
    public class ScrumInfoViewModel : INotifyPropertyChanged
    {
        private ScrumInfoModel _scrum = new ScrumInfoModel();
        private ScrumInfoModel _selectedScrum;
        private ObservableCollection<ScrumInfoModel> _scrumList;
        private readonly HttpClient _httpClient;
        private System.Timers.Timer _uiRefreshTimer;

        private string GetBaseUrl() => DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7073/"
            : "https://localhost:7073/";

        private string _passedUid = "ADMIN";
        private HubConnection _hubConnection;

        public ScrumInfoModel Scrum
        {
            get => _scrum;
            set
            {
                _scrum = value;
                OnPropertyChanged();
                SaveCommand?.ChangeCanExecute();
            }
        }

        public void NotifyScrumPropertyChanged()
        {
            OnPropertyChanged(nameof(Scrum));
            SaveCommand?.ChangeCanExecute();
        }

        public ScrumInfoModel SelectedScrum
        {
            get => _selectedScrum;
            set
            {
                _selectedScrum = value;
                OnPropertyChanged();
                if (_selectedScrum != null) LoadSelectedScrum(_selectedScrum);
            }
        }

        public ObservableCollection<ScrumInfoModel> ScrumList
        {
            get => _scrumList;
            set { _scrumList = value; OnPropertyChanged(); }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand EditCommand { get; }
        public RelayCommand ExitCommand { get; }

        public ScrumInfoViewModel(string uid = "ADMIN")
        {
            _passedUid = uid;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);

            Scrum = new ScrumInfoModel();
            ScrumList = new ObservableCollection<ScrumInfoModel>();

            AddCommand = new RelayCommand(_ => ExecuteAdd());
            SaveCommand = new RelayCommand(_ => ExecuteSave(), _ => CanExecuteSave());
            EditCommand = new RelayCommand(_ => ExecuteEdit(), _ => CanExecuteEdit());
            ExitCommand = new RelayCommand(_ => ExecuteExit());

            MessagingCenter.Subscribe<ScrumInfoModel>(this, "ScrumIDChanged", (sender) => {
                SaveCommand?.ChangeCanExecute();
            });

            // Initialize timer to check every 1 minute if 2 hours have passed for list items
            _uiRefreshTimer = new System.Timers.Timer(60000);
            _uiRefreshTimer.Elapsed += (s, e) => FilterListByTime();
            _uiRefreshTimer.Start();

            ResetForm();
            _ = LoadDataFromApiAsync();
            InitializeSignalR();
        }

        private void FilterListByTime()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    // Keep items that were entered within the last 2 hours
                    var cutoffTime = DateTime.Now.AddHours(-2);
                    var filteredList = ScrumList.Where(s => s.EntryDate >= cutoffTime).ToList();

                    if (filteredList.Count != ScrumList.Count)
                    {
                        ScrumList = new ObservableCollection<ScrumInfoModel>(filteredList);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Filtering error: {ex.Message}");
                }
            });
        }

        public void InitializeSignalR()
        {
            string baseUrl = GetBaseUrl();
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUrl}scrumHub", options =>
                {
                    options.HttpMessageHandlerFactory = (message) => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (m, cert, chain, errors) => true
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On("RefreshScrumList", () =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await LoadDataFromApiAsync();
                });
            });

            try { _ = _hubConnection.StartAsync(); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
        }

        private async Task LoadDataFromApiAsync()
        {
            try
            {
                string apiUrl = $"{GetBaseUrl()}api/ScrumInfo";
                var response = await _httpClient.GetFromJsonAsync<ObservableCollection<ScrumInfoModel>>(apiUrl);

                if (response != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ScrumList = response;
                    });
                }
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () => {
                    await Shell.Current.DisplayAlert("API Error", ex.Message, "OK");
                });
            }
        }

        private void ExecuteAdd() => ResetForm();

        private async void ExecuteSave()
        {
            try
            {
                string apiUrl = $"{GetBaseUrl()}api/ScrumInfo";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, Scrum);

                if (response.IsSuccessStatusCode)
                {
                    ResetForm();
                    await Shell.Current.DisplayAlert("Success", "Scrum information saved successfully!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("API Error", "Failed to save data.", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private bool CanExecuteSave() => !string.IsNullOrWhiteSpace(Scrum?.ScrumID);

        private async void ExecuteEdit()
        {
            if (SelectedScrum != null)
            {
                try
                {
                    string apiUrl = $"{GetBaseUrl()}api/ScrumInfo";
                    var response = await _httpClient.PutAsJsonAsync($"{apiUrl}/{Scrum.ScrumID}", Scrum);
                    if (response.IsSuccessStatusCode) await LoadDataFromApiAsync();
                    else await Shell.Current.DisplayAlert("API Error", "Failed to update.", "OK");
                }
                catch (Exception ex) { await Shell.Current.DisplayAlert("Error", ex.Message, "OK"); }
            }
        }

        private bool CanExecuteEdit() => SelectedScrum != null;

        private void ExecuteExit()
        {
            _uiRefreshTimer?.Stop();
            _uiRefreshTimer?.Dispose();
            Application.Current.Quit();
        }

        private void LoadSelectedScrum(ScrumInfoModel source)
        {
            Scrum.ScrumID = source.ScrumID;
            Scrum.ScrumDesc = source.ScrumDesc;
            NotifyScrumPropertyChanged();
        }

        private void ResetForm()
        {
            Scrum.ScrumID = string.Empty;
            Scrum.EntryDate = DateTime.Now;
            Scrum.EntryByUID = _passedUid;
            Scrum.Activate = "Y";
            NotifyScrumPropertyChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}