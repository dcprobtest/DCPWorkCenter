using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Net.Http.Json;
using System.Text.Json;
using Model_MAUI;

namespace ModelView_MAUI
{
    public class TechInfoViewModel : INotifyPropertyChanged
    {
        private TechInfoModel _tech;
        private TechInfoModel _selectedTech;
        private ObservableCollection<TechInfoModel> _techList;
        private readonly HttpClient _httpClient;

        private readonly string _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7073/"
            : "https://localhost:7073/";

        private string _apiUrl;
        private string _passedUid;

        public TechInfoModel Tech
        {
            get => _tech;
            set { _tech = value; OnPropertyChanged(); }
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
                    Tech.OnPropertyChanged(nameof(Tech.IsActivated));
                }
            }
        }

        public ObservableCollection<TechInfoModel> TechList
        {
            get => _techList;
            set { _techList = value; OnPropertyChanged(); }
        }

        public Command AddCommand { get; }
        public Command SaveCommand { get; }
        public Command EditCommand { get; }
        public Command ExitCommand { get; }

        public TechInfoViewModel() : this("USR001") { }

        public TechInfoViewModel(string uid)
        {
            _passedUid = uid;
            _apiUrl = $"{_baseUrl}api/TechInfo";

            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            _httpClient = new HttpClient(handler);

            Tech = new TechInfoModel { EntryByUid = _passedUid, EntryDate = DateTime.Now };
            TechList = new ObservableCollection<TechInfoModel>();

            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(ExecuteSave);
            EditCommand = new Command(ExecuteEdit);

            ExitCommand = new Command(async () => {
                if (Shell.Current != null) await Shell.Current.Navigation.PopAsync();
            });

            LoadApiData();
        }

        private void ExecuteAdd()
        {
            _selectedTech = null;
            OnPropertyChanged(nameof(SelectedTech));

            Tech = new TechInfoModel { EntryByUid = _passedUid, EntryDate = DateTime.Now, StartDate = DateTime.Now };
        }

        private async void ExecuteSave()
        {
            try
            {
                Tech.LastUpdate = DateTime.Now;
                var response = await _httpClient.PostAsJsonAsync(_apiUrl, Tech);

                if (response.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlert("Success", "Saved successfully!", "OK");

                    await Task.Delay(100);

                    LoadApiData();
                    ExecuteAdd();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    await Shell.Current.DisplayAlert("Server Error", error, "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void ExecuteEdit()
        {
            try
            {
                if (SelectedTech == null) return;
                Tech.LastUpdate = DateTime.Now;
                var response = await _httpClient.PutAsJsonAsync($"{_apiUrl}/{Tech.TechId}", Tech);

                if (response.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlert("Update", "Data updated successfully.", "OK");
                    await Task.Delay(100);
                    LoadApiData();
                    ExecuteAdd();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void LoadApiData()
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<ObservableCollection<TechInfoModel>>(_apiUrl);
                if (data != null)
                {
                    TechList = new ObservableCollection<TechInfoModel>(data);
                }
            }
            catch { }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}