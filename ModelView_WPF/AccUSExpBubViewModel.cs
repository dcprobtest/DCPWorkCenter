using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Model_WPF;
using ModelView_WPF.Helpers;

namespace ModelView_WPF
{
    public class AccUSExpBubViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private readonly HttpClient _httpClient;

        private bool _isEntryVisible;
        private AccUSExpBubModel _currentEntry;
        private ObservableCollection<AccUSExpBubModel> _expBubList;
        private ObservableCollection<string> _expCodeList;
        private string _selectedExpCode;

        private string _loggedInUID = string.Empty;

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

        public AccUSExpBubModel CurrentEntry
        {
            get => _currentEntry;
            set { _currentEntry = value; OnPropertyChanged(); }
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

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            ExpBubList = new ObservableCollection<AccUSExpBubModel>();
            ExpCodeList = new ObservableCollection<string>();
            CurrentEntry = new AccUSExpBubModel();

            IsEntryVisible = false;

            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(ExecuteSave, CanExecuteSave);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            LoadGridData();
            LoadDropdowns();
        }

        private async void LoadGridData()
        {
            try
            {
                var records = await _httpClient.GetFromJsonAsync<List<AccUSExpBubModel>>(ApiSettings.AccUSExpBubBaseUrl);
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
                MessageBox.Show($"Error loading grid data: {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadDropdowns()
        {
            try
            {
                var expHeads = await _httpClient.GetFromJsonAsync<List<AccUSExpendHeadModel>>(ApiSettings.AccUSExpBubGetExpCodesUrl);
                if (expHeads != null)
                {
                    ExpCodeList.Clear();
                    foreach (var head in expHeads)
                    {
                        if (!string.IsNullOrEmpty(head.ExpCode))
                        {
                            ExpCodeList.Add(head.ExpCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dropdowns: {ex.Message}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAdd(object parameter)
        {
            CurrentEntry = new AccUSExpBubModel
            {
                EntryDate = DateTime.Now,
                UID = _loggedInUID
            };
            SelectedExpCode = null;
            IsEntryVisible = true;
        }

        private async void ExecuteSave(object parameter)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiSettings.AccUSExpBubBaseUrl, CurrentEntry);

                if (response.IsSuccessStatusCode)
                {
                    LoadGridData();
                    IsEntryVisible = false;
                    CurrentEntry = new AccUSExpBubModel();
                    SelectedExpCode = null;
                    MessageBox.Show("Data saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to save data to server.\nError: {errorMsg}", "API Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing save operation: {ex.Message}", "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanExecuteSave(object parameter)
        {
            return CurrentEntry != null && !string.IsNullOrEmpty(CurrentEntry.ExpCode);
        }

        private void ExecuteEdit(object parameter)
        {
            if (parameter is AccUSExpBubModel selectedItem)
            {
                CurrentEntry = selectedItem;
                SelectedExpCode = selectedItem.ExpCode;
                IsEntryVisible = true;
            }
        }

        private bool CanExecuteEdit(object parameter)
        {
            return true;
        }

        private void ExecuteExit(object parameter)
        {
            if (parameter is Window window)
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
    }
}