using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ModelView_WPF.Helpers;

namespace ModelView_WPF
{
    public class STEMViewModel : INotifyPropertyChanged
    {
        // Property to hold the UID
        private string _adminID;
        public string AdminID
        {
            get => _adminID;
            set { _adminID = value; OnPropertyChanged(); }
        }

        // TransID will be auto-generated
        private string _transID;
        public string TransID
        {
            get => _transID;
            set { _transID = value; OnPropertyChanged(); }
        }

        private string _studentUID;
        public string StudentUID
        {
            get => _studentUID;
            set { _studentUID = value; OnPropertyChanged(); }
        }

        private string _dYear;
        public string dYear
        {
            get => _dYear;
            set { _dYear = value; OnPropertyChanged(); }
        }

        private string _dMonth;
        public string dMonth
        {
            get => _dMonth;
            set { _dMonth = value; OnPropertyChanged(); }
        }

        private DateTime? _payDate;
        public DateTime? PayDate
        {
            get => _payDate;
            set { _payDate = value; OnPropertyChanged(); }
        }

        private string _hours;
        public string Hours
        {
            get => _hours;
            set { _hours = value; OnPropertyChanged(); }
        }

        private double? _rate;
        public double? Rate
        {
            get => _rate;
            set { _rate = value; OnPropertyChanged(); }
        }

        private double? _sui;
        public double? SUI
        {
            get => _sui;
            set { _sui = value; OnPropertyChanged(); }
        }

        private double? _empTax;
        public double? EmpTax
        {
            get => _empTax;
            set { _empTax = value; OnPropertyChanged(); }
        }

        private double? _total;
        public double? Total
        {
            get => _total;
            set { _total = value; OnPropertyChanged(); }
        }

        private double? _paid;
        public double? Paid
        {
            get => _paid;
            set { _paid = value; OnPropertyChanged(); }
        }

        // To control the visibility of the right panel
        private bool _isInputVisible = false;
        public bool IsInputVisible
        {
            get => _isInputVisible;
            set { _isInputVisible = value; OnPropertyChanged(); }
        }

        // To hold the selected item from the DataGrid
        private STEMViewModel _selectedSTEM;
        public STEMViewModel SelectedSTEM
        {
            get => _selectedSTEM;
            set { _selectedSTEM = value; OnPropertyChanged(); }
        }

        private ObservableCollection<STEMViewModel> _stemList = new ObservableCollection<STEMViewModel>();
        public ObservableCollection<STEMViewModel> STEMList
        {
            get => _stemList;
            set { _stemList = value; OnPropertyChanged(); }
        }

        // Button Commands
        public ICommand SaveCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ExitCommand { get; }

        public STEMViewModel()
        {
            PayDate = DateTime.Now;

            // Initialize commands
            SaveCommand = new RelayCommand(async (obj) => await SaveSTEMDataAsync());

            AddCommand = new RelayCommand(async (obj) =>
            {
                ClearFields();
                IsInputVisible = true;
                await GenerateNextTransID(); // Checks data from API and fetches the ID
            });

            DeleteCommand = new RelayCommand((obj) => DeleteSTEMData());

            EditCommand = new RelayCommand((obj) => EditSTEMData());

            ExitCommand = new RelayCommand((obj) =>
            {
                IsInputVisible = false;
            });
        }

        // Method to edit data
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
                MessageBox.Show("Please select an item from the list to edit.");
            }
        }

        // Method to delete data
        private void DeleteSTEMData()
        {
            if (SelectedSTEM != null)
            {
                var result = MessageBox.Show("Are you sure to delete it?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    STEMList.Remove(SelectedSTEM);
                    MessageBox.Show("Item removed from list.");
                    IsInputVisible = false;
                }
            }
            else
            {
                MessageBox.Show("Please select an item from the list to delete.");
            }
        }

        // Method to clear input fields
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

        // Logic to auto-generate TransID (fetches latest ID from API)
        private async Task GenerateNextTransID()
        {
            try
            {
                // Localhost self-signed SSL certificate error bypass handler setup
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                using (HttpClient client = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) })
                {
                    var response = await client.GetAsync(ApiSettings.GetLastTransIDUrl());

                    if (response.IsSuccessStatusCode)
                    {
                        string lastIdStr = await response.Content.ReadAsStringAsync();
                        lastIdStr = lastIdStr.Trim('\"'); // Remove quotation marks

                        if (int.TryParse(lastIdStr, out int lastId))
                        {
                            // Adds 1 to the latest database number and formats to 6 digits
                            TransID = (lastId + 1).ToString("D6");
                        }
                        else
                        {
                            TransID = "000001";
                        }
                    }
                    else
                    {
                        TransID = "000001";
                    }
                }
            }
            catch (Exception)
            {
                TransID = "000001"; // Default if connection error occurs
            }
        }

        public void SetUID(string uid)
        {
            AdminID = uid;
        }

        public async Task<bool> SaveSTEMDataAsync()
        {
            try
            {
                // Localhost self-signed SSL certificate error bypass handler setup
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                using (HttpClient client = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) })
                {
                    var stemData = new
                    {
                        TransID = this.TransID,
                        AdminUID = this.AdminID,
                        StudentUID = this.StudentUID,
                        dYear = this.dYear,
                        dMonth = this.dMonth,
                        PayDate = this.PayDate,
                        Hours = this.Hours,
                        Rate = this.Rate,
                        SUI = this.SUI,
                        EmpTax = this.EmpTax,
                        Total = this.Total,
                        Paid = this.Paid
                    };

                    var response = await client.PostAsJsonAsync(ApiSettings.SaveSTEMUrl(), stemData);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Data Saved Successfully!");

                        STEMList.Add(new STEMViewModel
                        {
                            TransID = this.TransID,
                            StudentUID = this.StudentUID,
                            dYear = this.dYear,
                            dMonth = this.dMonth,
                            Hours = this.Hours,
                            Rate = this.Rate,
                            Total = this.Total,
                            Paid = this.Paid
                        });

                        IsInputVisible = false;
                        return true;
                    }
                    else
                    {
                        string errorResponse = await response.Content.ReadAsStringAsync();
                        MessageBox.Show("Failed to save data. Details: " + errorResponse);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
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