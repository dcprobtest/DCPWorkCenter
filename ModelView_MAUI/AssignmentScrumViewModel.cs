using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Model_MAUI;

namespace View_MAUI
{
    public class AssignmentScrumViewModel : INotifyPropertyChanged
    {
        private AssignmentScrumModel _scrum;
        private AssignmentScrumModel _selectedScrum;
        private bool _isActivated;
        private bool _isLoading = false;

        private readonly HttpClient _httpClient;

        // Using 10.0.2.2 for Android Emulator to access local machine
        private readonly string ApiUrl = (DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7073/api/AssignmentScrum"
            : "https://localhost:7073/api/AssignmentScrum");

        private readonly JsonSerializerOptions _jsonOptions;

        public ObservableCollection<int> AvailableAssignIDs { get; set; } = new();
        public ObservableCollection<string> AvailableScrumIDs { get; set; } = new();
        public ObservableCollection<AssignmentScrumModel> ScrumList { get; set; } = new();

        public AssignmentScrumModel Scrum
        {
            get => _scrum;
            set { _scrum = value; OnPropertyChanged(); }
        }

        public AssignmentScrumModel SelectedScrum
        {
            get => _selectedScrum;
            set
            {
                if (_selectedScrum != value)
                {
                    _selectedScrum = value;
                    OnPropertyChanged();

                    if (_isLoading || _selectedScrum == null) return;

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

        public bool IsActivated
        {
            get => _isActivated;
            set
            {
                if (_isActivated != value)
                {
                    _isActivated = value;
                    OnPropertyChanged();
                    if (Scrum != null) Scrum.Activate = _isActivated ? "Y" : "N";
                }
            }
        }

        public Command AddCommand { get; }
        public Command SaveCommand { get; }
        public Command EditCommand { get; }

        // Default constructor
        public AssignmentScrumViewModel() : this(string.Empty) { }

        // Constructor accepting UID
        public AssignmentScrumViewModel(string uid)
        {
            Scrum = new AssignmentScrumModel { StartDate = DateTime.Now, Activate = "N", AssignByUID = uid };

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            _httpClient = new HttpClient(handler);

            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(ExecuteSave);
            EditCommand = new Command(ExecuteEdit);

            LoadDataFromApi();
        }

        private void AppReset()
        {
            string currentUID = Scrum?.AssignByUID ?? string.Empty;
            Scrum = new AssignmentScrumModel { AssignByUID = currentUID, StartDate = DateTime.Now, Activate = "N" };
            IsActivated = false;
            SelectedScrum = null;
        }

        private void ExecuteAdd() => AppReset();

        private async void LoadDataFromApi()
        {
            if (_isLoading) return;
            try
            {
                _isLoading = true;
                var data = await _httpClient.GetFromJsonAsync<List<AssignmentScrumModel>>(ApiUrl, _jsonOptions);
                ScrumList.Clear();
                if (data != null)
                {
                    foreach (var item in data) ScrumList.Add(item);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally { _isLoading = false; }
        }

        private async void ExecuteSave()
        {
            var response = await _httpClient.PostAsJsonAsync(ApiUrl, Scrum);
            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("Success", "Saved successfully", "OK");
                LoadDataFromApi();
                AppReset();
            }
        }

        private async void ExecuteEdit()
        {
            var response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/UpdateStatus", Scrum);
            if (response.IsSuccessStatusCode)
            {
                await Shell.Current.DisplayAlert("Success", "Updated successfully", "OK");
                LoadDataFromApi();
                AppReset();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}