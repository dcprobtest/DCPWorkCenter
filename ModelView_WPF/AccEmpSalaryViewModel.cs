using Model_WPF;
using ModelView_WPF.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ModelView_WPF
{
    public class AccEmpSalaryViewModel : INotifyPropertyChanged
    {
        private AccEmpSalaryModel _selectedSalary;
        private string _monthID;
        private readonly HttpClient _httpClient;

        private ObservableCollection<AccExpBubModel> _expSubCodeList;
        public ObservableCollection<AccExpBubModel> ExpSubCodeList
        {
            get => _expSubCodeList;
            set { _expSubCodeList = value; OnPropertyChanged(); }
        }

        public ObservableCollection<AccEmpSalaryModel> SalaryList { get; set; }

        public AccEmpSalaryViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            SalaryList = new ObservableCollection<AccEmpSalaryModel>();
            ExpSubCodeList = new ObservableCollection<AccExpBubModel>();
            SelectedSalary = new AccEmpSalaryModel();

            AddCommand = new RelayCommand(ExecuteAdd);
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave(obj));
            EditCommand = new RelayCommand(ExecuteEdit);
            ExitCommand = new RelayCommand(ExecuteExit);

            LoadSalaries();
            LoadExpSubCodes();
        }

        public AccEmpSalaryModel SelectedSalary
        {
            get => _selectedSalary;
            set { _selectedSalary = value; OnPropertyChanged(); }
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

                SelectedSalary.ExpDate = new DateTime(year, nextMonth, 15);
                OnPropertyChanged(nameof(SelectedSalary));
            }
        }

        private async void LoadExpSubCodes()
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<List<AccExpBubModel>>(ApiSettings.AccEmpSalaryGetExpSubCodesUrl);
                if (response != null)
                {
                    ExpSubCodeList = new ObservableCollection<AccExpBubModel>(response);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Problem loading ComboBox data: {ex.Message}");
            }
        }

        private async void LoadSalaries()
        {
            try
            {
                var salaries = await _httpClient.GetFromJsonAsync<ObservableCollection<AccEmpSalaryModel>>(ApiSettings.AccEmpSalaryBaseUrl);
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
                MessageBox.Show($"Problem loading data: {ex.Message}");
            }
        }

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        private void ExecuteAdd(object obj)
        {
            SelectedSalary = new AccEmpSalaryModel();
            MonthID = string.Empty;
            OnPropertyChanged(nameof(SelectedSalary));
        }

        private async Task ExecuteSave(object obj)
        {
            if (SelectedSalary == null) return;

            try
            {
                HttpResponseMessage response;

                if (SelectedSalary.ExpenditurID == 0)
                {
                    response = await _httpClient.PostAsJsonAsync(ApiSettings.AccEmpSalaryBaseUrl, SelectedSalary);
                }
                else
                {
                    response = await _httpClient.PostAsJsonAsync(ApiSettings.UpdateAccEmpSalaryUrl(SelectedSalary.ExpenditurID), SelectedSalary);
                }

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Saved successfully!");
                    LoadSalaries();
                    ExecuteAdd(null);
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Could not save data to server.\nError: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API connection problem: {ex.Message}");
            }
        }

        private void ExecuteEdit(object obj)
        {
            if (obj is AccEmpSalaryModel model)
            {
                SelectedSalary = model;
                MonthID = model.MonthID;
                MessageBox.Show("Edit mode active.");
            }
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}