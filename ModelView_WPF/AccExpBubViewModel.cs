using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Linq; // LINQ যোগ করা হয়েছে অটো-জেনারেশনের জন্য
using Model_WPF;
using ModelView_WPF;

namespace ViewModel_WPF
{
    public class AccExpBubViewModel : INotifyPropertyChanged
    {
        private readonly string _apiUrl = "https://localhost:7073/api/AccExpBub";
        private readonly HttpClient _httpClient;
        private readonly string _receivedUid = "Admin"; // ডিফল্ট ভ্যালু হিসেবে "Admin" রাখা হলো

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
            set { _isEntryVisible = value; OnPropertyChanged(); }
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
            ExpCodeList = new ObservableCollection<string>(); // শুরুতে খালি থাকবে
            IsEntryVisible = false;

            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave(obj), CanExecuteAction);
            EditCommand = new RelayCommand(ExecuteEdit, CanExecuteAction);
            ExitCommand = new RelayCommand(ExecuteExit);

            LoadDataFromApi();
            LoadExpCodes(); // এই মেথডটি নতুন যোগ করা হয়েছে
        }

        // frmAccExpBub.xaml.cs থেকে uid রিসিভ করার জন্য নতুন কনস্ট্রাক্টর
        public AccExpBubViewModel(string uid) : this()
        {
            _receivedUid = uid; // রিসিভ করা uid লোকাল ভেরিয়েবলে সেট করা হলো
        }

        private async void LoadDataFromApi()
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<ObservableCollection<AccExpBubModel>>(_apiUrl);
                if (data != null) ExpBubList = data;
            }
            catch (Exception) { /* API রান না থাকলে এরর এড়াতে */ }
        }

        // API থেকে ExpCodes লোড করার মেথড
        private async void LoadExpCodes()
        {
            try
            {
                // আপনার নতুন API এন্ডপয়েন্ট কল করা হচ্ছে
                var codes = await _httpClient.GetFromJsonAsync<List<string>>("https://localhost:7073/api/AccExpBub/GetExpendHeadCodes");

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
                // ডাটা না আসলে বা এরর হলে হ্যান্ডেলিং
            }
        }

        #region Command Methods

        private void ExecuteAdd(object obj)
        {
            IsEntryVisible = true;

            // ১. অটো ExpSubCode জেনারেশন লজিক (001, 002 এভাবে)
            string nextSubCode = "001";
            if (ExpBubList != null && ExpBubList.Any())
            {
                var lastCode = ExpBubList.Max(x => int.Parse(x.ExpSubCode));
                nextSubCode = (lastCode + 1).ToString("D3"); // ৩ ডিজিটে ফরম্যাট করবে
            }

            // ২. নতুন এন্ট্রি তৈরি (হার্ডকোডেড "Admin" এর বদলে রিসিভ করা _receivedUid সেট করা হলো)
            CurrentEntry = new AccExpBubModel
            {
                EntryDate = DateTime.Now,
                UID = _receivedUid, // রিসিভ করা User ID এখানে সেট হবে এবং ফর্মে দেখাবে
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
                    var response = await _httpClient.PostAsJsonAsync(_apiUrl, CurrentEntry);

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
            if (IsEntryVisible == false) IsEntryVisible = true;
        }

        private void ExecuteExit(object obj)
        {
            Application.Current.Windows[0]?.Close();
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