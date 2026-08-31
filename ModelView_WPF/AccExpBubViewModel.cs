using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Net.Http;
using ModelView_WPF;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using Model_WPF;
using ModelView_WPF.Helpers;

namespace ViewModel_WPF
{
    public class AccExpBubViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private readonly string _receivedUid = "Admin";

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
                CommandManager.InvalidateRequerySuggested();
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
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            ExpBubList = new ObservableCollection<AccExpBubModel>();
            ExpCodeList = new ObservableCollection<string>();
            IsEntryVisible = false;

            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave(obj), CanExecuteAction);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteAction);
            ExitCommand = new RelayCommand(ExecuteExit);

            LoadDataFromApi();
            LoadExpCodes();
        }

        public AccExpBubViewModel(string uid) : this()
        {
            _receivedUid = uid;
        }

        private async void LoadDataFromApi()
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<ObservableCollection<AccExpBubModel>>(ApiSettings.AccExpBubBaseUrl);
                if (data != null) ExpBubList = data;
            }
            catch (Exception) { }
        }

        private async void LoadExpCodes()
        {
            try
            {
                var codes = await _httpClient.GetFromJsonAsync<List<string>>(ApiSettings.AccExpBubGetExpendHeadCodesUrl);

                if (codes != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
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
            }
        }

        #region Command Methods

        private void ExecuteAdd(object obj)
        {
            IsEntryVisible = true;

            string nextSubCode = "001";
            if (ExpBubList != null && ExpBubList.Any())
            {
                var lastCode = ExpBubList.Max(x => int.Parse(x.ExpSubCode));
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

        private async Task ExecuteSave(object obj)
        {
            if (CurrentEntry != null)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync(ApiSettings.AccExpBubBaseUrl, CurrentEntry);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Data Saved Successfully!");
                        ExpBubList.Add(CurrentEntry);
                        IsEntryVisible = false;
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error: {error}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Connection Error: {ex.Message}");
                }
            }
        }

        private void ExecuteEdit(object obj)
        {
            if (obj is AccExpBubModel model)
            {
                CurrentEntry = model;
                SelectedExpCode = model.ExpCode;
            }
            if (IsEntryVisible == false) IsEntryVisible = true;
        }

        private void ExecuteExit(object obj)
        {
            if (obj is Window window)
            {
                window.Close();
            }
            else
            {
                foreach (Window win in Application.Current.Windows)
                {
                    if (win.DataContext == this)
                    {
                        win.Close();
                        break;
                    }
                }
            }
        }

        private bool CanExecuteAction(object obj)
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