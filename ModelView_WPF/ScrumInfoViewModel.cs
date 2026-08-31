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
using ModelView_WPF.Helpers;

namespace ModelView_WPF
{
    public class ScrumInfoViewModel : INotifyPropertyChanged
    {
        private ScrumInfoModel _scrum;
        private ScrumInfoModel _selectedScrum;
        private ObservableCollection<ScrumInfoModel> _scrumList;
        private readonly HttpClient _httpClient;

        private string _passedUid = "ADMIN";

        private HubConnection _hubConnection;

        public ScrumInfoModel Scrum
        {
            get => _scrum;
            set
            {
                _scrum = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
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
                    LoadSelectedScrum(_selectedScrum);
                }
                CommandManager.InvalidateRequerySuggested();
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

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        public ScrumInfoViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            Scrum = new ScrumInfoModel();
            ScrumList = new ObservableCollection<ScrumInfoModel>();

            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            ResetForm();

            _ = LoadDataFromApiAsync();

            InitializeSignalR();
        }

        public ScrumInfoViewModel(string uid) : this()
        {
            if (!string.IsNullOrEmpty(uid))
            {
                _passedUid = uid;
                Scrum.EntryByUID = _passedUid;
            }
        }

        public async void InitializeSignalR()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(ApiSettings.ScrumHubUrl())
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On("RefreshScrumList", () =>
            {
                System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                {
                    await LoadDataFromApiAsync();
                });
            });

            try
            {
                await _hubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SignalR Connection Error: {ex.Message}");
            }
        }

        private async Task LoadDataFromApiAsync()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ObservableCollection<ScrumInfoModel>>(ApiSettings.GetScrumInfoRoute());
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

        private void ExecuteAdd(object parameter)
        {
            ResetForm();
            SelectedScrum = null;
        }

        private async void ExecuteSave(object parameter)
        {
            try
            {
                Scrum.LastUpdate = DateTime.Now;
                var response = await _httpClient.PostAsJsonAsync(ApiSettings.GetScrumInfoRoute(), Scrum);
                if (response.IsSuccessStatusCode)
                {
                    await LoadDataFromApiAsync();
                    ResetForm();
                    SelectedScrum = null;
                    MessageBox.Show("Data saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to save data. Status Code: {response.StatusCode}\n{errorMsg}", "API Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to API: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteSave(object parameter)
        {
            return !string.IsNullOrWhiteSpace(Scrum?.ScrumID);
        }

        private async void ExecuteEdit(object parameter)
        {
            if (SelectedScrum != null)
            {
                try
                {
                    Scrum.LastUpdate = DateTime.Now;
                    string putUrl = $"{ApiSettings.GetScrumInfoRoute()}/{Scrum.ScrumID}";
                    var response = await _httpClient.PutAsJsonAsync(putUrl, Scrum);

                    if (response.IsSuccessStatusCode)
                    {
                        await LoadDataFromApiAsync();
                        MessageBox.Show("Data updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Failed to update data. Status Code: {response.StatusCode}\n{errorMsg}", "API Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            return SelectedScrum != null && !string.IsNullOrWhiteSpace(Scrum?.ScrumID);
        }

        private void ExecuteExit(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
            else
            {
                foreach (Window win in System.Windows.Application.Current.Windows)
                {
                    if (win.DataContext == this)
                    {
                        win.Close();
                        break;
                    }
                }
            }
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
            Scrum = new ScrumInfoModel
            {
                ScrumID = string.Empty,
                ScrumDesc = string.Empty,
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                EntryByUID = _passedUid,
                LastUpdate = DateTime.Now,
                Activate = "Y"
            };
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}