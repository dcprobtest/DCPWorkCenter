using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Maui.Controls;
using Model_MAUI;

namespace View_MAUI
{
    public class AccExpBubViewModel : INotifyPropertyChanged
    {
        private readonly string _baseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7073"
            : "https://localhost:7073";

        private readonly string _apiUrl;
        private readonly HttpClient _httpClient;
        private string _receivedUid = "Admin";

        #region Properties

        private AccExpBubModel _currentEntry;
        public AccExpBubModel CurrentEntry
        {
            get => _currentEntry;
            set { _currentEntry = value; OnPropertyChanged(); }
        }

        private ObservableCollection<AccExpBubModel> _expBubList;
        public ObservableCollection<AccExpBubModel> ExpBubList
        {
            get => _expBubList;
            set { _expBubList = value; OnPropertyChanged(); }
        }

        private ObservableCollection<string> _expCodeList;
        public ObservableCollection<string> ExpCodeList
        {
            get => _expCodeList;
            set { _expCodeList = value; OnPropertyChanged(); }
        }

        private string _selectedExpCode;
        public string SelectedExpCode
        {
            get => _selectedExpCode;
            set
            {
                _selectedExpCode = value;
                if (CurrentEntry != null) CurrentEntry.ExpCode = value;
                OnPropertyChanged();
            }
        }

        private bool _isEntryVisible;
        public bool IsEntryVisible
        {
            get => _isEntryVisible;
            set
            {
                _isEntryVisible = value;
                OnPropertyChanged();
                ((Command)SaveCommand).ChangeCanExecute();
                ((Command)EditCommand).ChangeCanExecute();
            }
        }

        #endregion

        #region Commands
        public ICommand AddCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand ExitCommand { get; set; }
        #endregion

        public AccExpBubViewModel()
        {
            _apiUrl = $"{_baseUrl}/api/AccExpBub";
            _httpClient = new HttpClient();

            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async () => await ExecuteSave(), CanExecuteAction);
            EditCommand = new Command(ExecuteEdit, CanExecuteAction);
            ExitCommand = new Command(async () => await ExecuteExit());

            ExpBubList = new ObservableCollection<AccExpBubModel>();
            ExpCodeList = new ObservableCollection<string>();

            IsEntryVisible = false;

            Task.Run(async () =>
            {
                await LoadDataFromApi();
                await LoadExpCodes();
            });
        }

        public AccExpBubViewModel(string uid) : this()
        {
            _receivedUid = uid;
        }

        private async Task LoadDataFromApi()
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<ObservableCollection<AccExpBubModel>>(_apiUrl);
                if (data != null) ExpBubList = data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API Load Error: {ex.Message}");
            }
        }

        private async Task LoadExpCodes()
        {
            try
            {
                var codes = await _httpClient.GetFromJsonAsync<List<string>>($"{_baseUrl}/api/AccExpBub/GetExpendHeadCodes");

                if (codes != null)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ExpCodeList.Clear();
                        foreach (var code in codes)
                        {
                            ExpCodeList.Add(code);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load ExpCodes Error: {ex.Message}");
            }
        }

        #region Command Methods

        private void ExecuteAdd()
        {
            IsEntryVisible = true;

            string nextSubCode = "001";
            if (ExpBubList != null && ExpBubList.Any())
            {
                var lastCode = ExpBubList.Max(x => {
                    int.TryParse(x.ExpSubCode, out int numericCode);
                    return numericCode;
                });
                nextSubCode = (lastCode + 1).ToString("D3");
            }

            CurrentEntry = new AccExpBubModel
            {
                EntryDate = DateTime.Now,
                UID = _receivedUid,
                ExpSubCode = nextSubCode,
                ExpDetail = ""
            };

            SelectedExpCode = null;
        }

        private async Task ExecuteSave()
        {
            if (CurrentEntry == null) return;

            try
            {
                var response = await _httpClient.PostAsJsonAsync(_apiUrl, CurrentEntry);

                if (response.IsSuccessStatusCode)
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Success", "Data Saved Successfully!", "OK");

                    ExpBubList.Add(CurrentEntry);
                    IsEntryVisible = false;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Error", $"Error: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Connection Error", $"Connection Error: {ex.Message}", "OK");
            }
        }

        private void ExecuteEdit()
        {
            if (!IsEntryVisible) IsEntryVisible = true;
        }

        private async Task ExecuteExit()
        {
            if (Application.Current?.MainPage?.Navigation.NavigationStack.Count > 1)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            else
            {
                Application.Current?.Quit();
            }
        }

        private bool CanExecuteAction()
        {
            return IsEntryVisible;
        }

        #endregion

        #region PropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}