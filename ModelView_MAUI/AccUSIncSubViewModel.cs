using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Model_MAUI; // আপনার MAUI প্রজেক্টের আসল মডেল ক্লাসের নেমস্পেস এখানে রিপ্লেস করুন

namespace ModelView_MAUI
{
    public class AccUSIncSubViewModel : INotifyPropertyChanged
    {
        // Private Fields
        private string _incSubCode = string.Empty;
        private string _incDetail = string.Empty;
        private string _incCode = string.Empty;
        private string _uid = string.Empty;
        private DateTime _entryDate = DateTime.Now; // MAUI DatePicker-এর জন্য অ-নালযোগ্য রাখা হয়েছে
        private AccUSIncSubModel? _selectedUSIncSub;
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "http://74.208.227.170:8050/api/AccUSIncSub";

        // Collections for DataGrid and ComboBox
        public ObservableCollection<AccUSIncSubModel> USIncSubList { get; set; }
        public ObservableCollection<string> IncCodeList { get; set; }

        // ICommand Properties
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        // Default Constructor
        public AccUSIncSubViewModel()
        {
            _httpClient = new HttpClient();

            USIncSubList = new ObservableCollection<AccUSIncSubModel>();
            IncCodeList = new ObservableCollection<string>();

            // .NET MAUI স্ট্যান্ডার্ড কমান্ড আর্কিটেকচার
            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async () => await ExecuteSave(), CanExecuteSave);
            EditCommand = new Command(ExecuteEdit);
            ExitCommand = new Command(async () => await ExecuteExit());

            LoadInitialData();
        }

        // Parameterized Constructor to receive UID from View
        public AccUSIncSubViewModel(string uid)
        {
            _httpClient = new HttpClient();

            USIncSubList = new ObservableCollection<AccUSIncSubModel>();
            IncCodeList = new ObservableCollection<string>();

            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async () => await ExecuteSave(), CanExecuteSave);
            EditCommand = new Command(ExecuteEdit);
            ExitCommand = new Command(async () => await ExecuteExit());

            UID = uid;

            LoadInitialDataWithUID(uid);
        }

        #region Public Properties for View Binding
        public string IncSubCode
        {
            get => _incSubCode;
            set
            {
                if (_incSubCode != value)
                {
                    _incSubCode = value;
                    OnPropertyChanged();
                    // ইনপুট পরিবর্তনের সাথে সাথে SAVE বাটনের CanExecute লজিক ট্রিগার হবে
                    (SaveCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        public string IncDetail
        {
            get => _incDetail;
            set
            {
                if (_incDetail != value)
                {
                    _incDetail = value;
                    OnPropertyChanged();
                    (SaveCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        public string SelectedIncCode
        {
            get => _incCode;
            set
            {
                if (_incCode != value)
                {
                    _incCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public string UID
        {
            get => _uid;
            set
            {
                if (_uid != value)
                {
                    _uid = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime EntryDate
        {
            get => _entryDate;
            set
            {
                if (_entryDate != value)
                {
                    _entryDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public AccUSIncSubModel? SelectedUSIncSub
        {
            get => _selectedUSIncSub;
            set
            {
                if (_selectedUSIncSub != value)
                {
                    _selectedUSIncSub = value;
                    OnPropertyChanged();

                    // গ্রিড সিলেকশন চেঞ্জ হলে ডেটা ফর্ম ফিল্ডে চলে আসবে
                    if (_selectedUSIncSub != null)
                    {
                        IncSubCode = _selectedUSIncSub.IncSubCode ?? string.Empty;
                        SelectedIncCode = _selectedUSIncSub.IncCode ?? string.Empty;
                        IncDetail = _selectedUSIncSub.IncDetail ?? string.Empty;
                        UID = _selectedUSIncSub.UID ?? string.Empty;
                        EntryDate = _selectedUSIncSub.EntryDate ?? DateTime.Now;
                    }
                }
            }
        }
        #endregion

        #region Command Methods Execution
        private void ExecuteAdd()
        {
            IncSubCode = string.Empty;
            IncDetail = string.Empty;
            SelectedIncCode = IncCodeList.Count > 0 ? IncCodeList[0] : string.Empty;
            UID = "01";
            EntryDate = DateTime.Now;
            SelectedUSIncSub = null;
        }

        private async Task ExecuteSave()
        {
            var targetSubHead = new AccUSIncSubModel
            {
                IncSubCode = this.IncSubCode,
                IncCode = this.SelectedIncCode,
                IncDetail = this.IncDetail,
                UID = this.UID,
                EntryDate = this.EntryDate
            };

            try
            {
                HttpResponseMessage response;

                if (SelectedUSIncSub == null)
                {
                    response = await _httpClient.PostAsJsonAsync(ApiBaseUrl, targetSubHead);
                }
                else
                {
                    response = await _httpClient.PutAsJsonAsync($"{ApiBaseUrl}/{targetSubHead.IncSubCode}", targetSubHead);
                }

                if (response.IsSuccessStatusCode)
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Success", "Data Sync with Server Successful!", "OK");

                    await RefreshGridData();
                    ExecuteAdd();
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("API Error", $"Server Status Error: {response.StatusCode}\nDetails: {errorMsg}", "OK");
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Network Exception", $"Connection to API Failed: {ex.Message}", "OK");
            }
        }

        private bool CanExecuteSave()
        {
            return !string.IsNullOrWhiteSpace(IncSubCode) && !string.IsNullOrWhiteSpace(IncDetail);
        }

        private async void ExecuteEdit()
        {
            if (SelectedUSIncSub != null)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Edit Active", $"Editing Mode Loaded for Target Sub Code: {SelectedUSIncSub.IncSubCode}", "OK");
            }
        }

        private async Task ExecuteExit()
        {
            if (Application.Current?.MainPage?.Navigation.NavigationStack.Count > 1)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }
        #endregion

        #region Data Loading Helpers (API Integration)
        private async void LoadInitialData()
        {
            UID = "01";
            await FetchComboBoxCodes();
            await RefreshGridData();
        }

        private async void LoadInitialDataWithUID(string uid)
        {
            await FetchComboBoxCodes();
            await RefreshGridData();
            UID = uid;
        }

        private async Task FetchComboBoxCodes()
        {
            try
            {
                var rawCodes = await _httpClient.GetFromJsonAsync<List<string>>($"{ApiBaseUrl}/GetIncCodes");
                if (rawCodes != null)
                {
                    IncCodeList.Clear();
                    foreach (var code in rawCodes)
                    {
                        IncCodeList.Add(code);
                    }

                    if (IncCodeList.Count > 0)
                    {
                        SelectedIncCode = IncCodeList[0];
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"API fetch error: {ex.Message}");

                // Fallback local schema assignments if API target is unreachable
                IncCodeList.Clear();
                IncCodeList.Add("01");
                IncCodeList.Add("02");
                IncCodeList.Add("03");

                if (IncCodeList.Count > 0) SelectedIncCode = IncCodeList[0];
            }
        }

        private async Task RefreshGridData()
        {
            try
            {
                var items = await _httpClient.GetFromJsonAsync<List<AccUSIncSubModel>>(ApiBaseUrl);
                if (items != null)
                {
                    USIncSubList.Clear();
                    foreach (var record in items)
                    {
                        USIncSubList.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Fetch Exception", $"Failed to sync and refresh datagrid elements: {ex.Message}", "OK");
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}