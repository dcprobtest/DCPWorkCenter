using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Model_MAUI;
using Model_WPF; // আপনার MAUI প্রজেক্টের আসল মডেল ক্লাসের নেমস্পেস এখানে পরিবর্তন করুন

namespace ModelView_MAUI
{
    public class AccUSIncomeHeadViewModel : INotifyPropertyChanged
    {
        private bool _isEditMode = false;
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "http://74.208.227.170:8050/api/AccUSIncomeHead";
        private readonly string _currentUID = string.Empty;

        #region PropertiesBackedByFields

        private string _incCode;
        private string _incDetail;
        private string _uid;
        private DateTime _entryDate = DateTime.Now; // MAUI DatePicker-এর জন্য অ-নালযোগ্য করা হয়েছে
        private AccUSIncomeHeadModel _selectedUSIncome;
        private ObservableCollection<AccUSIncomeHeadModel> _usIncomeList;

        public string IncCode
        {
            get => _incCode;
            set
            {
                _incCode = value;
                OnPropertyChanged();
                // ইনপুট পরিবর্তনের সাথে সাথে SAVE বাটনের CanExecute লজিক ট্রিগার হবে
                (SaveCommand as Command)?.ChangeCanExecute();
            }
        }

        public string IncDetail
        {
            get => _incDetail;
            set { _incDetail = value; OnPropertyChanged(); }
        }

        public string UID
        {
            get => _uid;
            set { _uid = value; OnPropertyChanged(); }
        }

        public DateTime EntryDate
        {
            get => _entryDate;
            set { _entryDate = value; OnPropertyChanged(); }
        }

        public AccUSIncomeHeadModel SelectedUSIncome
        {
            get => _selectedUSIncome;
            set
            {
                _selectedUSIncome = value;
                OnPropertyChanged();

                if (_selectedUSIncome != null)
                {
                    IncCode = _selectedUSIncome.IncCode;
                    IncDetail = _selectedUSIncome.IncDetail;
                    UID = _selectedUSIncome.UID;
                    EntryDate = _selectedUSIncome.EntryDate ?? DateTime.Now;
                }

                // গ্রিড সিলেক্ট হওয়ার সাথে সাথে EDIT বাটনের CanExecute লজিক রিফ্রেশ হবে
                (EditCommand as Command)?.ChangeCanExecute();
            }
        }

        public ObservableCollection<AccUSIncomeHeadModel> USIncomeList
        {
            get => _usIncomeList;
            set { _usIncomeList = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        #endregion

        #region Constructors

        public AccUSIncomeHeadViewModel()
        {
            _httpClient = new HttpClient();
            USIncomeList = new ObservableCollection<AccUSIncomeHeadModel>();

            // .NET MAUI স্ট্যান্ডার্ড কমান্ড আর্কিটেকচার
            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async () => await ExecuteSave(), CanExecuteSave);
            EditCommand = new Command(ExecuteEdit, CanExecuteEdit);
            ExitCommand = new Command(async () => await ExecuteExit());

            ClearFields();
            LoadDataGridFromDatabase();
        }

        public AccUSIncomeHeadViewModel(string uid) : this()
        {
            _currentUID = uid;
            this.UID = uid;
        }

        #endregion

        #region Command Methods Implementation

        private void ExecuteAdd()
        {
            _isEditMode = false;
            ClearFields();

            UID = _currentUID;
            EntryDate = DateTime.Now;
        }

        private async Task ExecuteSave()
        {
            try
            {
                var inputData = new AccUSIncomeHeadModel
                {
                    IncCode = this.IncCode,
                    IncDetail = this.IncDetail,
                    UID = this.UID,
                    EntryDate = this.EntryDate
                };

                if (_isEditMode)
                {
                    HttpResponseMessage response = await _httpClient.PutAsJsonAsync($"{ApiUrl}/{IncCode}", inputData);

                    if (response.IsSuccessStatusCode)
                    {
                        if (SelectedUSIncome != null)
                        {
                            SelectedUSIncome.IncCode = IncCode;
                            SelectedUSIncome.IncDetail = IncDetail;
                            SelectedUSIncome.UID = UID;
                            SelectedUSIncome.EntryDate = EntryDate;
                        }

                        if (Application.Current?.MainPage != null)
                            await Application.Current.MainPage.DisplayAlert("Success", "Data updated successfully via API!", "OK");
                    }
                    else
                    {
                        if (Application.Current?.MainPage != null)
                            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to update data. Status Code: {response.StatusCode}", "OK");
                    }
                }
                else
                {
                    HttpResponseMessage response = await _httpClient.PostAsJsonAsync(ApiUrl, inputData);

                    if (response.IsSuccessStatusCode)
                    {
                        USIncomeList.Add(inputData);

                        if (Application.Current?.MainPage != null)
                            await Application.Current.MainPage.DisplayAlert("Success", "Data saved successfully via API!", "OK");
                    }
                    else
                    {
                        if (Application.Current?.MainPage != null)
                            await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save data. Status Code: {response.StatusCode}", "OK");
                    }
                }

                ClearFields();
                _isEditMode = false;
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("API Error", $"An error occurred while communicating with the API: {ex.Message}", "OK");
            }
        }

        private bool CanExecuteSave()
        {
            return !string.IsNullOrWhiteSpace(IncCode) && IncCode.Length <= 2;
        }

        private void ExecuteEdit()
        {
            if (SelectedUSIncome != null)
            {
                _isEditMode = true;
                // অলরেডি প্রোপার্টি অ্যাসাইনমেন্ট সিলেকশন চেঞ্জেই হ্যান্ডেল করা আছে
            }
        }

        private bool CanExecuteEdit()
        {
            return SelectedUSIncome != null;
        }

        private async Task ExecuteExit()
        {
            if (Application.Current?.MainPage?.Navigation.NavigationStack.Count > 1)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

        #endregion

        #region Helper Methods

        private void ClearFields()
        {
            IncCode = string.Empty;
            IncDetail = string.Empty;
            UID = _currentUID;
            EntryDate = DateTime.Now;
            SelectedUSIncome = null;
        }

        private async void LoadDataGridFromDatabase()
        {
            try
            {
                var items = await _httpClient.GetFromJsonAsync<ObservableCollection<AccUSIncomeHeadModel>>(ApiUrl);
                if (items != null)
                {
                    USIncomeList.Clear();
                    foreach (var item in items)
                    {
                        USIncomeList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Load Error", $"Failed to load data from API: {ex.Message}", "OK");
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}