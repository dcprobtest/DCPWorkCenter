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
using Model_MAUI; // আপনার MAUI প্রোজেক্টের আসল মডেল নেমস্পেস এখানে রি-ম্যাপ করুন

namespace ModelView_MAUI
{
    public class AccUSExpBubViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://74.208.227.170:8050/") };

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
                // বর্তমান এন্ট্রি অবজেক্ট পরিবর্তন হলে SAVE বাটনের CanExecute স্টেট চেক হবে
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
                    // কোড সিলেক্ট বা পরিবর্তন হলে SAVE কমান্ডকে এলার্ট করবে
                    (SaveCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        // CollectionView থেকে সিলেক্টেড রো ট্র্যাকিংয়ের জন্য প্রোপার্টি
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

        // MAUI Standard ICommands
        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        // Default Constructor
        public AccUSExpBubViewModel() : this(string.Empty)
        {
        }

        // Parameterized Constructor
        public AccUSExpBubViewModel(string uid)
        {
            _loggedInUID = uid;

            ExpBubList = new ObservableCollection<AccUSExpBubModel>();
            ExpCodeList = new ObservableCollection<string>();
            CurrentEntry = new AccUSExpBubModel { EntryDate = DateTime.Now };

            // MAUI Core Command mappings
            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async () => await ExecuteSave(), CanExecuteSave);
            EditCommand = new Command(ExecuteEdit);
            ExitCommand = new Command(async () => await ExecuteExit());

            IsEntryVisible = false;

            // ডেটা লোড প্রসেস ইনিশিয়েট
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
                {
                    await Application.Current.MainPage.DisplayAlert("API Error", $"Error loading grid data: {ex.Message}", "OK");
                }
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
                        {
                            ExpCodeList.Add(head.ExpCode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("API Error", $"Error loading dropdowns: {ex.Message}", "OK");
                }
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
                    {
                        await Application.Current.MainPage.DisplayAlert("Success", "Data saved successfully.", "OK");
                    }

                    LoadGridData();
                    IsEntryVisible = false;
                    CurrentEntry = new AccUSExpBubModel { EntryDate = DateTime.Now, UID = _loggedInUID };
                    SelectedExpCode = null;
                }
                else
                {
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("API Warning", "Failed to save data to server.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Exception", $"Error executing save operation: {ex.Message}", "OK");
                }
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
                // রেফারেন্স সেফ রাখার জন্য অবজেক্ট ক্লোন বা রি-অ্যাসাইন
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

            // ন্যাভিগেশন স্ট্যাক চেক করে পেজ পপ করা অথবা অ্যাপ ক্লোজ করা
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