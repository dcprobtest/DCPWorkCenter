using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using View_WPF.Model;
using ModelView_WPF;
using ModelView_WPF.Helpers;
using Newtonsoft.Json.Serialization;

namespace View_WPF.ModelView_WPF
{
    public class CountryModel
    {
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
    }

    public class StateModel
    {
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public string CountryCode { get; set; }
    }

    public class DepartmentDropdownModel
    {
        public string DeptCode { get; set; }
        public string DeptName { get; set; }
        public string DisplayName => $"{DeptCode} - {DeptName}";
    }

    public class DesignationDropdownModel
    {
        public string DesigCode { get; set; }
        public string DesigName { get; set; }
        public string DisplayName => $"{DesigCode} - {DesigName}";
    }

    public class EmpInfoViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;

        public List<CountryModel> Countries { get; set; } = new List<CountryModel>
        {
            new CountryModel { CountryCode = "USA", CountryName = "USA" },
            new CountryModel { CountryCode = "BAN", CountryName = "Bangladesh" },
            new CountryModel { CountryCode = "IND", CountryName = "India" },
            new CountryModel { CountryCode = "PAK", CountryName = "Pakistan" }
        };

        private List<StateModel> _allStates = new List<StateModel>
        {
            new StateModel { Code = "AL", DisplayName = "AL – Alabama", CountryCode = "USA" },
            new StateModel { Code = "AK", DisplayName = "AK – Alaska", CountryCode = "USA" },
            new StateModel { Code = "AZ", DisplayName = "AZ – Arizona", CountryCode = "USA" },
            new StateModel { Code = "AR", DisplayName = "AR – Arkansas", CountryCode = "USA" },
            new StateModel { Code = "CA", DisplayName = "CA – California", CountryCode = "USA" },
            new StateModel { Code = "CO", DisplayName = "CO – Colorado", CountryCode = "USA" },
            new StateModel { Code = "CT", DisplayName = "CT – Connecticut", CountryCode = "USA" },
            new StateModel { Code = "DE", DisplayName = "DE – Delaware", CountryCode = "USA" },
            new StateModel { Code = "FL", DisplayName = "FL – Florida", CountryCode = "USA" },
            new StateModel { Code = "GA", DisplayName = "GA – Georgia", CountryCode = "USA" },
            new StateModel { Code = "HI", DisplayName = "HI – Hawaii", CountryCode = "USA" },
            new StateModel { Code = "ID", DisplayName = "ID – Idaho", CountryCode = "USA" },
            new StateModel { Code = "IL", DisplayName = "IL – Illinois", CountryCode = "USA" },
            new StateModel { Code = "IN", DisplayName = "IN – Indiana", CountryCode = "USA" },
            new StateModel { Code = "IA", DisplayName = "IA – Iowa", CountryCode = "USA" },
            new StateModel { Code = "KS", DisplayName = "KS – Kansas", CountryCode = "USA" },
            new StateModel { Code = "KY", DisplayName = "KY – Kentucky", CountryCode = "USA" },
            new StateModel { Code = "LA", DisplayName = "LA – Louisiana", CountryCode = "USA" },
            new StateModel { Code = "ME", DisplayName = "ME – Maine", CountryCode = "USA" },
            new StateModel { Code = "MD", DisplayName = "MD – Maryland", CountryCode = "USA" },
            new StateModel { Code = "MA", DisplayName = "MA – Massachusetts", CountryCode = "USA" },
            new StateModel { Code = "MI", DisplayName = "MI – Michigan", CountryCode = "USA" },
            new StateModel { Code = "MN", DisplayName = "MN – Minnesota", CountryCode = "USA" },
            new StateModel { Code = "MS", DisplayName = "MS – Mississippi", CountryCode = "USA" },
            new StateModel { Code = "MO", DisplayName = "MO – Missouri", CountryCode = "USA" },
            new StateModel { Code = "MT", DisplayName = "MT – Montana", CountryCode = "USA" },
            new StateModel { Code = "NE", DisplayName = "NE – Nebraska", CountryCode = "USA" },
            new StateModel { Code = "NV", DisplayName = "NV – Nevada", CountryCode = "USA" },
            new StateModel { Code = "NH", DisplayName = "NH – New Hampshire", CountryCode = "USA" },
            new StateModel { Code = "NJ", DisplayName = "NJ – New Jersey", CountryCode = "USA" },
            new StateModel { Code = "NM", DisplayName = "NM – New Mexico", CountryCode = "USA" },
            new StateModel { Code = "NY", DisplayName = "NY – New York", CountryCode = "USA" },
            new StateModel { Code = "NC", DisplayName = "NC – North Carolina", CountryCode = "USA" },
            new StateModel { Code = "ND", DisplayName = "ND – North Dakota", CountryCode = "USA" },
            new StateModel { Code = "OH", DisplayName = "OH – Ohio", CountryCode = "USA" },
            new StateModel { Code = "OK", DisplayName = "OK – Oklahoma", CountryCode = "USA" },
            new StateModel { Code = "OR", DisplayName = "OR – Oregon", CountryCode = "USA" },
            new StateModel { Code = "PA", DisplayName = "PA – Pennsylvania", CountryCode = "USA" },
            new StateModel { Code = "RI", DisplayName = "RI – Rhode Island", CountryCode = "USA" },
            new StateModel { Code = "SC", DisplayName = "SC – South Carolina", CountryCode = "USA" },
            new StateModel { Code = "SD", DisplayName = "SD – South Dakota", CountryCode = "USA" },
            new StateModel { Code = "TN", DisplayName = "TN – Tennessee", CountryCode = "USA" },
            new StateModel { Code = "TX", DisplayName = "TX – Texas", CountryCode = "USA" },
            new StateModel { Code = "UT", DisplayName = "UT – Utah", CountryCode = "USA" },
            new StateModel { Code = "VT", DisplayName = "VT – Vermont", CountryCode = "USA" },
            new StateModel { Code = "VA", DisplayName = "VA – Virginia", CountryCode = "USA" },
            new StateModel { Code = "WA", DisplayName = "WA – Washington", CountryCode = "USA" },
            new StateModel { Code = "WV", DisplayName = "WV – West Virginia", CountryCode = "USA" },
            new StateModel { Code = "WI", DisplayName = "WI – Wisconsin", CountryCode = "USA" },
            new StateModel { Code = "WY", DisplayName = "WY – Wyoming", CountryCode = "USA" },

            new StateModel { Code = "DHA", DisplayName = "DHA – Dhaka", CountryCode = "BAN" },
            new StateModel { Code = "CTG", DisplayName = "CTG – Chittagong", CountryCode = "BAN" },
            new StateModel { Code = "SYL", DisplayName = "SYL – Sylhet", CountryCode = "BAN" },
            new StateModel { Code = "RAJ", DisplayName = "RAJ – Rajshahi", CountryCode = "BAN" },
            new StateModel { Code = "KHU", DisplayName = "KHU – Khulna", CountryCode = "BAN" },
            new StateModel { Code = "BAR", DisplayName = "BAR – Barisal", CountryCode = "BAN" },
            new StateModel { Code = "RAN", DisplayName = "RAN – Rangpur", CountryCode = "BAN" },
            new StateModel { Code = "MYM", DisplayName = "MYM – Mymensingh", CountryCode = "BAN" },

            new StateModel { Code = "DL", DisplayName = "DL – Delhi", CountryCode = "IND" },
            new StateModel { Code = "MH", DisplayName = "MH – Maharashtra", CountryCode = "IND" },
            new StateModel { Code = "KA", DisplayName = "KA – Karnataka", CountryCode = "IND" },
            new StateModel { Code = "TN_IN", DisplayName = "TN – Tamil Nadu", CountryCode = "IND" },
            new StateModel { Code = "WB", DisplayName = "WB – West Bengal", CountryCode = "IND" },

            new StateModel { Code = "PB", DisplayName = "PB – Punjab", CountryCode = "PAK" },
            new StateModel { Code = "SD_PK", DisplayName = "SD – Sindh", CountryCode = "PAK" },
            new StateModel { Code = "KP", DisplayName = "KP – Khyber Pakhtunkhwa", CountryCode = "PAK" },
            new StateModel { Code = "BA", DisplayName = "BA – Balochistan", CountryCode = "PAK" }
        };

        private ObservableCollection<StateModel> _states = new ObservableCollection<StateModel>();
        public ObservableCollection<StateModel> States
        {
            get => _states;
            set { _states = value; OnPropertyChanged(nameof(States)); }
        }

        private string _selectedCountryCode;
        public string SelectedCountryCode
        {
            get => _selectedCountryCode;
            set
            {
                _selectedCountryCode = value;
                OnPropertyChanged(nameof(SelectedCountryCode));
                if (!string.IsNullOrEmpty(_selectedCountryCode))
                {
                    Emp.Country = _selectedCountryCode;
                    LoadStates(_selectedCountryCode);
                }
                else
                {
                    Emp.Country = null;
                    States.Clear();
                }
            }
        }

        private ObservableCollection<DepartmentDropdownModel> _departments = new ObservableCollection<DepartmentDropdownModel>();
        public ObservableCollection<DepartmentDropdownModel> Departments
        {
            get => _departments;
            set { _departments = value; OnPropertyChanged(nameof(Departments)); }
        }

        private string _selectedDeptCode;
        public string SelectedDeptCode
        {
            get => _selectedDeptCode;
            set
            {
                _selectedDeptCode = value;
                OnPropertyChanged(nameof(SelectedDeptCode));
                if (!string.IsNullOrEmpty(_selectedDeptCode))
                {
                    Emp.DeptCode = _selectedDeptCode;
                }
                else
                {
                    Emp.DeptCode = null;
                }
            }
        }

        private ObservableCollection<DesignationDropdownModel> _designations = new ObservableCollection<DesignationDropdownModel>();
        public ObservableCollection<DesignationDropdownModel> Designations
        {
            get => _designations;
            set { _designations = value; OnPropertyChanged(nameof(Designations)); }
        }

        private string _selectedDesigCode;
        public string SelectedDesigCode
        {
            get => _selectedDesigCode;
            set
            {
                _selectedDesigCode = value;
                OnPropertyChanged(nameof(SelectedDesigCode));
                if (!string.IsNullOrEmpty(_selectedDesigCode))
                {
                    Emp.DesigCode = _selectedDesigCode;
                }
                else
                {
                    Emp.DesigCode = null;
                }
            }
        }

        private string _selectedStateString;
        public string SelectedStateString
        {
            get => _selectedStateString;
            set
            {
                _selectedStateString = value;
                OnPropertyChanged(nameof(SelectedStateString));

                if (!string.IsNullOrEmpty(_selectedStateString))
                {
                    Emp.State = _selectedStateString;
                }
                else
                {
                    Emp.State = null;
                }
            }
        }

        private EmpInfoModel _emp = new EmpInfoModel();
        public EmpInfoModel Emp
        {
            get => _emp;
            set { _emp = value; OnPropertyChanged(nameof(Emp)); }
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
                Emp.Activate = value ? "Y" : "N";
            }
        }

        public ICommand SaveCommand { get; }

        public EmpInfoViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            Emp.EntryDate = DateTime.Now;
            Emp.StartDate = DateTime.Now;
            Emp.EndDate = DateTime.Now;
            Emp.DOB = DateTime.Now;

            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());

            _ = LoadDropdownDataAsync();
        }

        public EmpInfoViewModel(string uid)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            Emp.EntryDate = DateTime.Now;
            Emp.StartDate = DateTime.Now;
            Emp.EndDate = DateTime.Now;
            Emp.DOB = DateTime.Now;
            Emp.UID = uid;

            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());

            _ = LoadDropdownDataAsync();
        }

        private void LoadStates(string countryCode)
        {
            States.Clear();
            var filteredStates = _allStates.Where(s => s.CountryCode == countryCode).ToList();
            foreach (var state in filteredStates)
            {
                States.Add(state);
            }
            SelectedStateString = null;
        }

        private async Task LoadDropdownDataAsync()
        {
            try
            {
                var deptResponse = await _httpClient.GetAsync(ApiSettings.GetDepartmentsUrl);
                if (deptResponse.IsSuccessStatusCode)
                {
                    var deptJson = await deptResponse.Content.ReadAsStringAsync();
                    var deptList = JsonConvert.DeserializeObject<List<DepartmentDropdownModel>>(deptJson);
                    if (deptList != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Departments.Clear();
                            foreach (var item in deptList)
                            {
                                Departments.Add(item);
                            }
                        });
                    }
                }
                else
                {
                    MessageBox.Show($"Dept API Failed: {deptResponse.StatusCode}");
                }

                var desigResponse = await _httpClient.GetAsync(ApiSettings.GetDesignationsUrl);
                if (desigResponse.IsSuccessStatusCode)
                {
                    var desigJson = await desigResponse.Content.ReadAsStringAsync();
                    var desigList = JsonConvert.DeserializeObject<List<DesignationDropdownModel>>(desigJson);
                    if (desigList != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Designations.Clear();
                            foreach (var item in desigList)
                            {
                                Designations.Add(item);
                            }
                        });
                    }
                }
                else
                {
                    MessageBox.Show($"Desig API Failed: {desigResponse.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("API Exception: " + ex.Message);
            }
        }

        private async Task ExecuteSave()
        {
            if (string.IsNullOrEmpty(Emp.UID))
            {
                MessageBox.Show("Please enter User ID (UID).");
                return;
            }

            try
            {
                var settings = new JsonSerializerSettings
                {
                    DateFormatString = "yyyy-MM-dd",
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                var json = JsonConvert.SerializeObject(Emp, settings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(ApiSettings.SaveEmployeeUrl(), content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Employee Information Saved Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ResetForm();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to save. Error: {error}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetForm()
        {
            Emp = new EmpInfoModel();
            IsActive = false;
            Emp.EntryDate = DateTime.Now;
            Emp.StartDate = DateTime.Now;
            Emp.EndDate = DateTime.Now;
            Emp.DOB = DateTime.Now;
            SelectedCountryCode = null;
            SelectedStateString = null;
            SelectedDeptCode = null;
            SelectedDesigCode = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}