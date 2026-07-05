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
using Model_MAUI; // আপনার প্রজেক্টের সঠিক মডেল নেমস্পেস

namespace ModelView_MAUI
{
    public class AccExpBubViewModel : INotifyPropertyChanged
    {
        private readonly string _apiUrl = "http://74.208.227.170:8050/api/AccExpBub";
        private readonly HttpClient _httpClient;
        private string _receivedUid = "Admin"; // ডিফল্ট ভ্যালু হিসেবে "Admin" রাখা হলো

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
                // IsEntryVisible পরিবর্তন হলে বাটনগুলোর CanExecute রিফ্রেশ করার জন্য
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
            _httpClient = new HttpClient();

            ExpBubList = new ObservableCollection<AccExpBubModel>();
            ExpCodeList = new ObservableCollection<string>();
            IsEntryVisible = false;

            // MAUI বিল্ট-ইন Command প্যাটার্ন
            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async () => await ExecuteSave(), CanExecuteAction);
            EditCommand = new Command(ExecuteEdit, CanExecuteAction);
            ExitCommand = new Command(async () => await ExecuteExit());

            LoadDataFromApi();
            LoadExpCodes();
        }

        // অন্য পেজ বা লগইন থেকে UID পাস করার জন্য কনস্ট্রাক্টর
        public AccExpBubViewModel(string uid) : this()
        {
            _receivedUid = uid;
            if (CurrentEntry != null)
            {
                CurrentEntry.UID = uid;
            }
        }

        private async void LoadDataFromApi()
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

        private async void LoadExpCodes()
        {
            try
            {
                var codes = await _httpClient.GetFromJsonAsync<List<string>>("http://74.208.227.170:8050/api/AccExpBub/GetExpendHeadCodes");

                if (codes != null)
                {
                    // MAUI থ্রেড সেফ ডিসপ্যাচার লজিক
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

            // অটো ExpSubCode জেনারেশন লজিক
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