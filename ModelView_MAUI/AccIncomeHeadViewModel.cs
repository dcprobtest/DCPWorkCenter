using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices; 
using Model_MAUI;

namespace View_MAUI
{
    public class AccIncomeHeadViewModel : INotifyPropertyChanged
    {
        private string _incCode = string.Empty;
        private string _incDetail = string.Empty;
        private string _uid = string.Empty;
        private DateTime _entryDate = DateTime.Now;
        private AccIncomeHeadModel _selectedIncome;
        private readonly HttpClient _httpClient;

        public ObservableCollection<AccIncomeHeadModel> IncomeList { get; set; }

        public AccIncomeHeadViewModel()
        {
            // URL logic updated to match AccEmpSalaryViewModel
            string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "https://10.0.2.2:7073/"
                : "https://localhost:7073/";

            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

            IncomeList = new ObservableCollection<AccIncomeHeadModel>();

            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async () => await ExecuteSave(), CanExecuteSave);
            EditCommand = new Command(async () => await ExecuteEdit(), CanExecuteEdit);
            ExitCommand = new Command(async () => await ExecuteExit());

            LoadInitialData();
        }

        // Parameterized constructor to receive UID from Form
        public AccIncomeHeadViewModel(string uid) : this()
        {
            this.UID = uid;
        }

        #region Properties Linked to View Fields

        public string IncCode
        {
            get => _incCode;
            set
            {
                _incCode = value;
                OnPropertyChanged();
                // Refresh Save button state when user types
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }

        public string IncDetail
        {
            get => _incDetail;
            set
            {
                _incDetail = value;
                OnPropertyChanged();
            }
        }

        public string UID
        {
            get => _uid;
            set
            {
                _uid = value;
                OnPropertyChanged();
            }
        }

        public DateTime EntryDate
        {
            get => _entryDate;
            set
            {
                _entryDate = value;
                OnPropertyChanged();
            }
        }

        public AccIncomeHeadModel SelectedIncome
        {
            get => _selectedIncome;
            set
            {
                _selectedIncome = value;
                OnPropertyChanged();
                if (_selectedIncome != null)
                {
                    IncCode = _selectedIncome.IncCode;
                    IncDetail = _selectedIncome.IncDetail;
                    UID = _selectedIncome.UID;
                    EntryDate = _selectedIncome.EntryDate ?? DateTime.Now;
                }
                // Refresh Edit button state when grid row is selected
                ((Command)EditCommand).ChangeCanExecute();
            }
        }

        #endregion

        #region Commands

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        #endregion

        #region Command Implementation Logic

        private async void LoadInitialData()
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<ObservableCollection<AccIncomeHeadModel>>("api/AccIncomeHead");
                if (data != null)
                {
                    IncomeList.Clear();
                    foreach (var item in data)
                    {
                        IncomeList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Data Load Error", $"Failed to load initial data from API: {ex.Message}", "OK");
                }
            }
        }

        private void ExecuteAdd()
        {
            IncCode = string.Empty;
            IncDetail = string.Empty;
            EntryDate = DateTime.Now;
            SelectedIncome = null;
        }

        private bool CanExecuteSave()
        {
            return !string.IsNullOrWhiteSpace(IncCode) && IncCode.Length == 2;
        }

        private async Task ExecuteSave()
        {
            try
            {
                var newIncome = new AccIncomeHeadModel
                {
                    IncCode = this.IncCode,
                    IncDetail = this.IncDetail,
                    UID = this.UID,
                    EntryDate = this.EntryDate
                };

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/AccIncomeHead", newIncome);

                if (response.IsSuccessStatusCode)
                {
                    IncomeList.Add(newIncome);
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Success", "Data saved successfully through API endpoint.", "OK");
                    }
                    ExecuteAdd();
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", $"API Error: {errorMessage}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save record: {ex.Message}", "OK");
                }
            }
        }

        private bool CanExecuteEdit()
        {
            return SelectedIncome != null;
        }

        private async Task ExecuteEdit()
        {
            try
            {
                if (SelectedIncome != null)
                {
                    SelectedIncome.IncCode = this.IncCode;
                    SelectedIncome.IncDetail = this.IncDetail;
                    SelectedIncome.UID = this.UID;
                    SelectedIncome.EntryDate = this.EntryDate;

                    HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"api/AccIncomeHead/{IncCode}", SelectedIncome);

                    if (response.IsSuccessStatusCode)
                    {
                        int index = IncomeList.IndexOf(SelectedIncome);
                        if (index >= 0)
                        {
                            IncomeList[index] = SelectedIncome;
                        }

                        if (Application.Current?.MainPage != null)
                        {
                            await Application.Current.MainPage.DisplayAlert("Updated", "Data saved successfully.", "OK");
                        }
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        if (Application.Current?.MainPage != null)
                        {
                            await Application.Current.MainPage.DisplayAlert("Error", $"API Error: {errorMessage}", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to modify record: {ex.Message}", "OK");
            }
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

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}