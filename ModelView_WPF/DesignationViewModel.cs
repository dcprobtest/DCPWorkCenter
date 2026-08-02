using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using Model_WPF;
using ModelView_WPF.Helpers;
using System.ComponentModel;
using System.Windows;

namespace ModelView_WPF
{
    public class DesignationViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;

        private DesignationModel _designation = new DesignationModel();
        public DesignationModel Designation
        {
            get => _designation;
            set
            {
                _designation = value;
                OnPropertyChanged(nameof(Designation));
            }
        }

        private bool _isActiveChecked;
        public bool IsActiveChecked
        {
            get => _isActiveChecked;
            set
            {
                _isActiveChecked = value;
                if (Designation != null)
                {
                    // Set 'Y' or 'N' according to API logic
                    Designation.Activate = value ? "Y" : "N";
                }
                OnPropertyChanged(nameof(IsActiveChecked));
            }
        }

        public ICommand SaveCommand { get; }

        public DesignationViewModel()
        {
            // Localhost self-signed SSL certificate error bypass handler setup
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            // HttpClient configuration updated with ApiSettings.BaseUrl
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            ResetForm();
            // Bind Save logic using RelayCommand
            SaveCommand = new RelayCommand(async (obj) => await SaveDesignation());
        }

        private async Task SaveDesignation()
        {
            // Validation: Cannot save without DesigCode
            if (string.IsNullOrWhiteSpace(Designation.DesigCode))
            {
                MessageBox.Show("Please enter a Designation Code.");
                return;
            }

            try
            {
                // Update LastUpdate to match API model requirements
                Designation.LastUpdate = DateTime.Now;

                var json = JsonConvert.SerializeObject(Designation);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send data to API using ApiSettings endpoint
                var response = await _httpClient.PostAsync(ApiSettings.SaveDesignationUrl(), content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Designation Saved Successfully!");
                    ResetForm();
                }
                else
                {
                    string errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error: {response.StatusCode}\nDetails: {errorMsg}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect to API.\nException: {ex.Message}");
            }
        }

        private void ResetForm()
        {
            // Reset form for a new entry
            Designation = new DesignationModel
            {
                DesigCode = string.Empty,
                Description = string.Empty,
                Responsiblity = string.Empty,
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = null,
                EntryByUid = "Admin", // Can be made dynamic if required
                LastUpdate = DateTime.Now,
                Activate = "N"
            };
            IsActiveChecked = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}