using System;
using System.Collections.Generic;
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
    public class AccUSExpBubViewModel : INotifyPropertyChanged
    {
        // BaseAddress logic as per requirement
        private readonly HttpClient _httpClient;

        private bool _isEntryVisible;
        private AccUSExpBubModel _currentEntry;
        private ObservableCollection<AccUSExpBubModel> _expBubList;
        private ObservableCollection<string> _expCodeList;
        private string _selectedExpCode;
        private AccUSExpBubModel _selectedExpBub;
        private string _loggedInUID = string.Empty;

        // Public Properties for Data Binding
        public bool IsEntryVisible
        {
            get => _isEntryVisible;
            set { _isEntryVisible = value; OnPropertyChanged(); }
        }

        public AccUSExpBubModel CurrentEntry
        {
            get => _currentEntry;
            set
            {
                _currentEntry = value;
                OnPropertyChanged();
                (SaveCommand as Command)?.ChangeCanExecute();
            }
        }

        public ObservableCollection<AccUSExpBubModel> ExpBubList
        {
            get => _expBubList;
            set { _expBubList = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> ExpCodeList
        {
            get => _expCodeList;
            set { _expCodeList = value; OnPropertyChanged(); }
        }

        public string SelectedExpCode
        {
            get => _selectedExpCode;
            set
            {
                _selectedExpCode = value;
                OnPropertyChanged();
                if (CurrentEntry != null)
                {
                    CurrentEntry.ExpCode = value;
                    (SaveCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        public AccUSExpBubModel SelectedExpBub
        {
            get => _selectedExpBub;
            set
            {
                _selectedExpBub = value;
                OnPropertyChanged();
                if (_selectedExpBub != null)
                {
                    ExecuteEdit();
                }
            }
        }

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        public AccUSExpBubViewModel() : this(string.Empty)
        {
        }

        public AccUSExpBubViewModel(string uid)
        {
            _loggedInUID = uid;

            // URL handling logic
            string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "https://10.0.2.2:7073/"
                : "https://localhost:7073/";

            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

            ExpBubList = new ObservableCollection<AccUSExpBubModel>();
            ExpCodeList = new ObservableCollection<string>();

            
            CurrentEntry = new AccUSExpBubModel { EntryDate = DateTime.Now, UID = _loggedInUID };

            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async () => await ExecuteSave(), CanExecuteSave);
            EditCommand = new Command(ExecuteEdit);
            ExitCommand = new Command(async () => await ExecuteExit());

            IsEntryVisible = false;

            LoadGridData();
            LoadDropdowns();
        }

        private async void LoadGridData()
        {
            try
            {
                var records = await _httpClient.GetFromJsonAsync<List<AccUSExpBubModel>>("api/AccUSExpBub");
                if (records != null)
                {
                    ExpBubList.Clear();
                    foreach (var item in records)
                    {
                        ExpBubList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("API Error", $"Error loading grid data: {ex.Message}", "OK");
            }
        }

        private async void LoadDropdowns()
        {
            try
            {
                var expHeads = await _httpClient.GetFromJsonAsync<List<AccUSExpendHeadModel>>("api/AccUSExpBub/GetExpCodes");
                if (expHeads != null)
                {
                    ExpCodeList.Clear();
                    foreach (var head in expHeads)
                    {
                        if (!string.IsNullOrEmpty(head.ExpCode))
                            ExpCodeList.Add(head.ExpCode);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("API Error", $"Error loading dropdowns: {ex.Message}", "OK");
            }
        }

        private void ExecuteAdd()
        {
            
            CurrentEntry = new AccUSExpBubModel
            {
                EntryDate = DateTime.Now,
                UID = _loggedInUID
            };
            SelectedExpCode = null;
            IsEntryVisible = true;
        }

        private async Task ExecuteSave()
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/AccUSExpBub", CurrentEntry);

                if (response.IsSuccessStatusCode)
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Success", "Data saved successfully.", "OK");

                    LoadGridData();
                    IsEntryVisible = false;
                    CurrentEntry = new AccUSExpBubModel { EntryDate = DateTime.Now, UID = _loggedInUID };
                    SelectedExpCode = null;
                }
                else
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("API Warning", "Failed to save data to server.", "OK");
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Exception", $"Error executing save operation: {ex.Message}", "OK");
            }
        }

        private bool CanExecuteSave()
        {
            return CurrentEntry != null && !string.IsNullOrEmpty(CurrentEntry.ExpCode);
        }

        private void ExecuteEdit()
        {
            if (SelectedExpBub != null)
            {
                CurrentEntry = new AccUSExpBubModel
                {
                    ExpSubCode = SelectedExpBub.ExpSubCode,
                    ExpCode = SelectedExpBub.ExpCode,
                    ExpDetail = SelectedExpBub.ExpDetail,
                    UID = SelectedExpBub.UID,
                    EntryDate = SelectedExpBub.EntryDate
                };
                SelectedExpCode = SelectedExpBub.ExpCode;
                IsEntryVisible = true;
            }
        }

        private async Task ExecuteExit()
        {
            IsEntryVisible = false;
            if (Application.Current?.MainPage?.Navigation.NavigationStack.Count > 1)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}