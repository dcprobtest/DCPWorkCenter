using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class StateModel
    {
        public string Code { get; set; }
        public string DisplayName { get; set; }
    }

    public class EmpInfoViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;

        public List<StateModel> States { get; set; } = new List<StateModel>
        {
            new StateModel { Code = "AL", DisplayName = "AL – Alabama" },
            new StateModel { Code = "AK", DisplayName = "AK – Alaska" },
            new StateModel { Code = "AZ", DisplayName = "AZ – Arizona" },
            new StateModel { Code = "AR", DisplayName = "AR – Arkansas" },
            new StateModel { Code = "CA", DisplayName = "CA – California" },
            new StateModel { Code = "CO", DisplayName = "CO – Colorado" },
            new StateModel { Code = "CT", DisplayName = "CT – Connecticut" },
            new StateModel { Code = "DE", DisplayName = "DE – Delaware" },
            new StateModel { Code = "FL", DisplayName = "FL – Florida" },
            new StateModel { Code = "GA", DisplayName = "GA – Georgia" },
            new StateModel { Code = "HI", DisplayName = "HI – Hawaii" },
            new StateModel { Code = "ID", DisplayName = "ID – Idaho" },
            new StateModel { Code = "IL", DisplayName = "IL – Illinois" },
            new StateModel { Code = "IN", DisplayName = "IN – Indiana" },
            new StateModel { Code = "IA", DisplayName = "IA – Iowa" },
            new StateModel { Code = "KS", DisplayName = "KS – Kansas" },
            new StateModel { Code = "KY", DisplayName = "KY – Kentucky" },
            new StateModel { Code = "LA", DisplayName = "LA – Louisiana" },
            new StateModel { Code = "ME", DisplayName = "ME – Maine" },
            new StateModel { Code = "MD", DisplayName = "MD – Maryland" },
            new StateModel { Code = "MA", DisplayName = "MA – Massachusetts" },
            new StateModel { Code = "MI", DisplayName = "MI – Michigan" },
            new StateModel { Code = "MN", DisplayName = "MN – Minnesota" },
            new StateModel { Code = "MS", DisplayName = "MS – Mississippi" },
            new StateModel { Code = "MO", DisplayName = "MO – Missouri" },
            new StateModel { Code = "MT", DisplayName = "MT – Montana" },
            new StateModel { Code = "NE", DisplayName = "NE – Nebraska" },
            new StateModel { Code = "NV", DisplayName = "NV – Nevada" },
            new StateModel { Code = "NH", DisplayName = "NH – New Hampshire" },
            new StateModel { Code = "NJ", DisplayName = "NJ – New Jersey" },
            new StateModel { Code = "NM", DisplayName = "NM – New Mexico" },
            new StateModel { Code = "NY", DisplayName = "NY – New York" },
            new StateModel { Code = "NC", DisplayName = "NC – North Carolina" },
            new StateModel { Code = "ND", DisplayName = "ND – North Dakota" },
            new StateModel { Code = "OH", DisplayName = "OH – Ohio" },
            new StateModel { Code = "OK", DisplayName = "OK – Oklahoma" },
            new StateModel { Code = "OR", DisplayName = "OR – Oregon" },
            new StateModel { Code = "PA", DisplayName = "PA – Pennsylvania" },
            new StateModel { Code = "RI", DisplayName = "RI – Rhode Island" },
            new StateModel { Code = "SC", DisplayName = "SC – South Carolina" },
            new StateModel { Code = "SD", DisplayName = "SD – South Dakota" },
            new StateModel { Code = "TN", DisplayName = "TN – Tennessee" },
            new StateModel { Code = "TX", DisplayName = "TX – Texas" },
            new StateModel { Code = "UT", DisplayName = "UT – Utah" },
            new StateModel { Code = "VT", DisplayName = "VT – Vermont" },
            new StateModel { Code = "VA", DisplayName = "VA – Virginia" },
            new StateModel { Code = "WA", DisplayName = "WA – Washington" },
            new StateModel { Code = "WV", DisplayName = "WV – West Virginia" },
            new StateModel { Code = "WI", DisplayName = "WI – Wisconsin" },
            new StateModel { Code = "WY", DisplayName = "WY – Wyoming" }
        };

        private string _selectedStateString;
        public string SelectedStateString
        {
            get => _selectedStateString;
            set
            {
                _selectedStateString = value;
                OnPropertyChanged(nameof(SelectedStateString));

                // Since SelectedValuePath="Code" is specified, the value will directly be the short code (e.g., "NY")
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
            // Localhost self-signed SSL certificate error bypass handler setup
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            // HttpClient configuration updated with ApiSettings.BaseUrl
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            Emp.EntryDate = DateTime.Now;
            Emp.StartDate = DateTime.Now;
            Emp.EndDate = DateTime.Now;
            Emp.DOB = DateTime.Now;

            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
        }

        public EmpInfoViewModel(string uid)
        {
            // Localhost self-signed SSL certificate error bypass handler setup
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            // HttpClient configuration updated with ApiSettings.BaseUrl
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            Emp.EntryDate = DateTime.Now;
            Emp.StartDate = DateTime.Now;
            Emp.EndDate = DateTime.Now;
            Emp.DOB = DateTime.Now;
            Emp.UID = uid;

            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
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

                // Saving employee information using ApiSettings endpoint
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
            SelectedStateString = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}