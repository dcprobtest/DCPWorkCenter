using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Devices;

namespace View_MAUI
{
    public class STEMViewModel : INotifyPropertyChanged
    {
        // Set the base URL dynamically for Android Emulator or local development
        private string GetBaseUrl() => DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7073/"
            : "https://localhost:7073/";

        // Properties
        private string _adminID;
        public string AdminID { get => _adminID; set { _adminID = value; OnPropertyChanged(); } }

        private string _transID;
        public string TransID { get => _transID; set { _transID = value; OnPropertyChanged(); } }

        private string _studentUID;
        public string StudentUID { get => _studentUID; set { _studentUID = value; OnPropertyChanged(); } }

        private string _dYear;
        public string dYear { get => _dYear; set { _dYear = value; OnPropertyChanged(); } }

        private string _dMonth;
        public string dMonth { get => _dMonth; set { _dMonth = value; OnPropertyChanged(); } }

        private DateTime? _payDate;
        public DateTime? PayDate { get => _payDate; set { _payDate = value; OnPropertyChanged(); } }

        private string _hours;
        public string Hours { get => _hours; set { _hours = value; OnPropertyChanged(); } }

        private double? _rate;
        public double? Rate { get => _rate; set { _rate = value; OnPropertyChanged(); } }

        private double? _sui;
        public double? SUI { get => _sui; set { _sui = value; OnPropertyChanged(); } }

        private double? _empTax;
        public double? EmpTax { get => _empTax; set { _empTax = value; OnPropertyChanged(); } }

        private double? _total;
        public double? Total { get => _total; set { _total = value; OnPropertyChanged(); } }

        private double? _paid;
        public double? Paid { get => _paid; set { _paid = value; OnPropertyChanged(); } }

        private bool _isInputVisible = false;
        public bool IsInputVisible { get => _isInputVisible; set { _isInputVisible = value; OnPropertyChanged(); } }

        private STEMViewModel _selectedSTEM;
        public STEMViewModel SelectedSTEM { get => _selectedSTEM; set { _selectedSTEM = value; OnPropertyChanged(); } }

        private ObservableCollection<STEMViewModel> _stemList = new ObservableCollection<STEMViewModel>();
        public ObservableCollection<STEMViewModel> STEMList { get => _stemList; set { _stemList = value; OnPropertyChanged(); } }

        // Commands
        public ICommand SaveCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        public STEMViewModel(string uid = "")
        {
            AdminID = uid;
            PayDate = DateTime.Now;

            SaveCommand = new Command(async () => await SaveSTEMDataAsync());

            AddCommand = new Command(async () =>
            {
                ClearFields();
                IsInputVisible = true;
                await GenerateNextTransID();
            });

            DeleteCommand = new Command(DeleteSTEMData);
            EditCommand = new Command(EditSTEMData);
            ExitCommand = new Command(() => IsInputVisible = false);
        }

        private void EditSTEMData()
        {
            if (SelectedSTEM != null)
            {
                TransID = SelectedSTEM.TransID;
                StudentUID = SelectedSTEM.StudentUID;
                dYear = SelectedSTEM.dYear;
                dMonth = SelectedSTEM.dMonth;
                PayDate = SelectedSTEM.PayDate;
                Hours = SelectedSTEM.Hours;
                Rate = SelectedSTEM.Rate;
                SUI = SelectedSTEM.SUI;
                EmpTax = SelectedSTEM.EmpTax;
                Total = SelectedSTEM.Total;
                Paid = SelectedSTEM.Paid;

                IsInputVisible = true;
            }
            else
            {
                Shell.Current.DisplayAlert("Alert", "Please select an item from the list to edit.", "OK");
            }
        }

        private async void DeleteSTEMData()
        {
            if (SelectedSTEM != null)
            {
                bool answer = await Shell.Current.DisplayAlert("Confirmation", "Are you sure to delete it?", "Yes", "No");
                if (answer)
                {
                    STEMList.Remove(SelectedSTEM);
                    await Shell.Current.DisplayAlert("Success", "Item removed from list.", "OK");
                    IsInputVisible = false;
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Alert", "Please select an item from the list to delete.", "OK");
            }
        }

        private void ClearFields()
        {
            StudentUID = string.Empty;
            dYear = string.Empty;
            dMonth = string.Empty;
            Hours = string.Empty;
            Rate = null;
            SUI = null;
            EmpTax = null;
            Total = null;
            Paid = null;
            PayDate = DateTime.Now;
        }

        private async Task GenerateNextTransID()
        {
            try
            {
                var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
                using (HttpClient client = new HttpClient(handler))
                {
                    string apiUrl = $"{GetBaseUrl()}api/Data/GetLastTransID";
                    var response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string lastIdStr = await response.Content.ReadAsStringAsync();
                        lastIdStr = lastIdStr.Trim('\"');

                        if (int.TryParse(lastIdStr, out int lastId))
                            TransID = (lastId + 1).ToString("D6");
                        else
                            TransID = "000001";
                    }
                    else
                        TransID = "000001";
                }
            }
            catch (Exception)
            {
                TransID = "000001";
            }
        }

        public async Task<bool> SaveSTEMDataAsync()
        {
            try
            {
                var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
                using (HttpClient client = new HttpClient(handler))
                {
                    string apiUrl = $"{GetBaseUrl()}api/Data/SaveSTEM";
                    var stemData = new
                    {
                        TransID,
                        AdminUID = AdminID,
                        StudentUID,
                        dYear,
                        dMonth,
                        PayDate,
                        Hours,
                        Rate,
                        SUI,
                        EmpTax,
                        Total,
                        Paid
                    };

                    var response = await client.PostAsJsonAsync(apiUrl, stemData);

                    if (response.IsSuccessStatusCode)
                    {
                        await Shell.Current.DisplayAlert("Success", "Data Saved Successfully!", "OK");
                        STEMList.Add(new STEMViewModel(AdminID) { TransID = this.TransID, StudentUID = this.StudentUID, dYear = this.dYear, dMonth = this.dMonth, Hours = this.Hours, Rate = this.Rate, Total = this.Total, Paid = this.Paid });
                        IsInputVisible = false;
                        return true;
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        await Shell.Current.DisplayAlert("Error", "Failed to save data. Details: " + errorResponse, "OK");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", "An error occurred: " + ex.Message, "OK");
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}