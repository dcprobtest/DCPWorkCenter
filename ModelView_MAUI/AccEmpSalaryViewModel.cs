using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Model_MAUI;

namespace View_MAUI
{
    public class AccEmpSalaryViewModel : INotifyPropertyChanged
    {
        private AccEmpSalaryModel _selectedSalary;
        private string _monthID;
        private AccExpBubModel _selectedExpSubCodeItem;
        private readonly HttpClient _httpClient;

        // Property to hold the received UID
        private string _accountUserId;
        public string AccountUserId
        {
            get => _accountUserId;
            set { _accountUserId = value; OnPropertyChanged(); }
        }

        private ObservableCollection<AccExpBubModel> _expSubCodeList;
        public ObservableCollection<AccExpBubModel> ExpSubCodeList
        {
            get => _expSubCodeList;
            set { _expSubCodeList = value; OnPropertyChanged(); }
        }

        public ObservableCollection<AccEmpSalaryModel> SalaryList { get; set; }

        // Updated constructor to accept uid
        public AccEmpSalaryViewModel(string uid)
        {
            // Set the UID
            AccountUserId = uid;

            // Using 10.0.2.2 for Android Emulator to access local machine
            string baseUrl = DeviceInfo.Platform == DevicePlatform.Android
                ? "https://10.0.2.2:7073/"
                : "https://localhost:7073/";

            _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };

            SalaryList = new ObservableCollection<AccEmpSalaryModel>();
            ExpSubCodeList = new ObservableCollection<AccExpBubModel>();
            SelectedSalary = new AccEmpSalaryModel { EntryDate = DateTime.Now };

            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async (obj) => await ExecuteSave(obj));
            EditCommand = new Command(ExecuteEdit);
            ExitCommand = new Command(async (obj) => await ExecuteExit(obj));

            LoadSalaries();
            LoadExpSubCodes();
        }

        public AccEmpSalaryModel SelectedSalary
        {
            get => _selectedSalary;
            set { _selectedSalary = value; OnPropertyChanged(); }
        }

        public AccExpBubModel SelectedExpSubCodeItem
        {
            get => _selectedExpSubCodeItem;
            set
            {
                _selectedExpSubCodeItem = value;
                OnPropertyChanged();
                if (_selectedExpSubCodeItem != null && SelectedSalary != null)
                {
                    SelectedSalary.ExpSubCode = _selectedExpSubCodeItem.ExpSubCode;
                }
            }
        }

        public string MonthID
        {
            get => _monthID;
            set
            {
                _monthID = value;
                OnPropertyChanged();
                UpdateExpDate();
                if (SelectedSalary != null)
                {
                    SelectedSalary.MonthID = value;
                }
            }
        }

        private void UpdateExpDate()
        {
            if (int.TryParse(_monthID, out int month) && month >= 1 && month <= 12)
            {
                int year = DateTime.Now.Year;
                int nextMonth = month + 1;

                if (nextMonth > 12)
                {
                    nextMonth = 1;
                    year++;
                }

                if (SelectedSalary != null)
                {
                    SelectedSalary.ExpDate = new DateTime(year, nextMonth, 15);
                    OnPropertyChanged(nameof(SelectedSalary));
                }
            }
        }

        private async void LoadExpSubCodes()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<AccExpBubModel>>("api/AccEmpSalary/GetExpSubCodes");
                if (response != null)
                {
                    ExpSubCodeList = new ObservableCollection<AccExpBubModel>(response);
                    SyncPickerSelectedValue();
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"ComboBox Error: {ex.Message}", "OK");
            }
        }

        private async void LoadSalaries()
        {
            try
            {
                var salaries = await _httpClient.GetFromJsonAsync<ObservableCollection<AccEmpSalaryModel>>("api/AccEmpSalary");
                if (salaries != null)
                {
                    SalaryList.Clear();
                    foreach (var item in salaries)
                    {
                        SalaryList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Data Load Error: {ex.Message}", "OK");
            }
        }

        private void SyncPickerSelectedValue()
        {
            if (SelectedSalary != null && ExpSubCodeList != null)
            {
                SelectedExpSubCodeItem = ExpSubCodeList.FirstOrDefault(x => x.ExpSubCode == SelectedSalary.ExpSubCode);
            }
        }

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        private void ExecuteAdd(object obj)
        {
            SelectedSalary = new AccEmpSalaryModel { EntryDate = DateTime.Now };
            MonthID = string.Empty;
            SelectedExpSubCodeItem = null;
            OnPropertyChanged(nameof(SelectedSalary));
        }

        private async Task ExecuteSave(object obj)
        {
            if (SelectedSalary == null) return;

            try
            {
                HttpResponseMessage response;
                if (SelectedSalary.ExpenditurID == 0)
                    response = await _httpClient.PostAsJsonAsync("api/AccEmpSalary", SelectedSalary);
                else
                    response = await _httpClient.PostAsJsonAsync($"api/AccEmpSalary/{SelectedSalary.ExpenditurID}", SelectedSalary);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Saved successfully!", "OK");
                    LoadSalaries();
                    ExecuteAdd(null);
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Server Error", $"Error: {errorMsg}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Connection Error", $"API problem: {ex.Message}", "OK");
            }
        }

        private async void ExecuteEdit(object obj)
        {
            if (obj is AccEmpSalaryModel model)
            {
                SelectedSalary = model;
                MonthID = model.MonthID;
                SyncPickerSelectedValue();
                await Application.Current.MainPage.DisplayAlert("System", "Edit mode active.", "OK");
            }
        }

        private async Task ExecuteExit(object obj)
        {
            if (Application.Current?.MainPage?.Navigation.NavigationStack.Count > 1)
                await Application.Current.MainPage.Navigation.PopAsync();
            else
                Application.Current?.Quit();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}