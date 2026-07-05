using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Model_MAUI; // আপনার MAUI প্রজেক্টের সঠিক মডেল নেমস্পেস নিশ্চিত করুন

namespace ModelView_MAUI
{
    public class AccIncomeHeadViewModel : INotifyPropertyChanged
    {
        private string _incCode = string.Empty;
        private string _incDetail = string.Empty;
        private string _uid = string.Empty;
        private DateTime _entryDate = DateTime.Now;
        private AccIncomeHeadModel _selectedIncome;

        // API Endpoint Configuration
        private static readonly HttpClient httpClient = new HttpClient { BaseAddress = new Uri("http://74.208.227.170:8050/") };

        // Collection to hold database records for the CollectionView
        public ObservableCollection<AccIncomeHeadModel> IncomeList { get; set; }

        public AccIncomeHeadViewModel()
        {
            IncomeList = new ObservableCollection<AccIncomeHeadModel>();

            // MAUI Standard Commands with CanExecute handling
            AddCommand = new Command(ExecuteAdd);
            SaveCommand = new Command(async () => await ExecuteSave(), CanExecuteSave);
            EditCommand = new Command(async () => await ExecuteEdit(), CanExecuteEdit);
            ExitCommand = new Command(async () => await ExecuteExit());

            // Load existing data from the database via API when initialized
            LoadInitialData();
        }

        // Parameterized constructor to receive UID from Form
        public AccIncomeHeadViewModel(string uid) : this()
        {
            this.UID = uid; // Sets received UID to show on the form
        }

        #region Properties Linked to View Fields

        public string IncCode
        {
            get => _incCode;
            set
            {
                _incCode = value;
                OnPropertyChanged();
                // রিফ্রেশ বাটন স্টেট যখন ইউজার টাইপ করছে
                ((Command)SaveCommand).ChangeCanExecute();
            }
        }

        public string IncDetail
        {
            get => _incDetail;
            set
            {
                _incDetail = value;
                OnPropertyChanged();
            }
        }

        public string UID
        {
            get => _uid;
            set
            {
                _uid = value;
                OnPropertyChanged();
            }
        }

        public DateTime EntryDate
        {
            get => _entryDate;
            set
            {
                _entryDate = value;
                OnPropertyChanged();
            }
        }

        public AccIncomeHeadModel SelectedIncome
        {
            get => _selectedIncome;
            set
            {
                _selectedIncome = value;
                OnPropertyChanged();
                if (_selectedIncome != null)
                {
                    IncCode = _selectedIncome.IncCode;
                    IncDetail = _selectedIncome.IncDetail;
                    UID = _selectedIncome.UID;
                    EntryDate = _selectedIncome.EntryDate ?? DateTime.Now;
                }
                // রিফ্রেশ এডিট বাটন স্টেট যখন গ্রিড রো সিলেক্ট হচ্ছে
                ((Command)EditCommand).ChangeCanExecute();
            }
        }

        #endregion

        #region Commands

        public ICommand AddCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        #endregion

        #region Command Implementation Logic

        // Fetch data from API and populate CollectionView
        private async void LoadInitialData()
        {
            try
            {
                var data = await httpClient.GetFromJsonAsync<ObservableCollection<AccIncomeHeadModel>>("api/AccIncomeHead");
                if (data != null)
                {
                    IncomeList.Clear();
                    foreach (var item in data)
                    {
                        IncomeList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Data Load Error", $"Failed to load initial data from API: {ex.Message}", "OK");
                }
            }
        }

        private void ExecuteAdd()
        {
            // Reset fields for a new entry setup
            IncCode = string.Empty;
            IncDetail = string.Empty;
            EntryDate = DateTime.Now;
            SelectedIncome = null;
        }

        private bool CanExecuteSave()
        {
            // Validation: Save button is enabled only when IncCode is exactly 2 characters
            return !string.IsNullOrWhiteSpace(IncCode) && IncCode.Length == 2;
        }

        private async Task ExecuteSave()
        {
            try
            {
                var newIncome = new AccIncomeHeadModel
                {
                    IncCode = this.IncCode,
                    IncDetail = this.IncDetail,
                    UID = this.UID,
                    EntryDate = this.EntryDate
                };

                // HTTP POST request to API to insert into database
                HttpResponseMessage response = await httpClient.PostAsJsonAsync("api/AccIncomeHead", newIncome);

                if (response.IsSuccessStatusCode)
                {
                    IncomeList.Add(newIncome);
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Success", "Data saved successfully through API endpoint.", "OK");
                    }
                    ExecuteAdd(); // Clear fields after a successful save operation
                }
                else
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", $"API Error: {errorMessage}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save record: {ex.Message}", "OK");
                }
            }
        }

        private bool CanExecuteEdit()
        {
            // Edit button remains enabled only when an existing grid row is selected
            return SelectedIncome != null;
        }

        private async Task ExecuteEdit()
        {
            try
            {
                if (SelectedIncome != null)
                {
                    SelectedIncome.IncCode = this.IncCode;
                    SelectedIncome.IncDetail = this.IncDetail;
                    SelectedIncome.UID = this.UID;
                    SelectedIncome.EntryDate = this.EntryDate;

                    // HTTP PUT request to API to update database row using IncCode as key
                    HttpResponseMessage response = await httpClient.PutAsJsonAsync($"api/AccIncomeHead/{IncCode}", SelectedIncome);

                    if (response.IsSuccessStatusCode)
                    {
                        // Refreshing collection bound to CollectionView layout
                        int index = IncomeList.IndexOf(SelectedIncome);
                        if (index >= 0)
                        {
                            IncomeList[index] = SelectedIncome;
                        }

                        if (Application.Current?.MainPage != null)
                        {
                            await Application.Current.MainPage.DisplayAlert("Updated", "Data saved successfully.", "OK");
                        }
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        if (Application.Current?.MainPage != null)
                        {
                            await Application.Current.MainPage.DisplayAlert("Error", $"API Error: {errorMessage}", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to modify record: {ex.Message}", "OK");
            }
        }

        private async Task ExecuteExit()
        {
            // Closes active view container safely using Navigation Stack or safely Quits app
            if (Application.Current?.MainPage?.Navigation.NavigationStack.Count > 1)
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            else
            {
                Application.Current?.Quit();
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}