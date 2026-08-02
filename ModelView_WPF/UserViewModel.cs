using ModelView_WPF;
using ModelView_WPF.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace View_WPF.ModelView_WPF
{
    public class UserViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;

        // Properties
        public string UID { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime EntryDate { get; set; } = DateTime.Now;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; }

        public ICommand SaveCommand { get; }

        public UserViewModel()
        {
            // Initialize HttpClient with SSL certificate bypass handler and ApiSettings BaseAddress
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };
            _apiUrl = ApiSettings.SaveUserUrl();

            // RelayCommand from your separate file is utilized here
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
        }

        private async Task ExecuteSave()
        {
            // Object is created aligning with the property names of the API's TblUser model
            var userData = new
            {
                Uid = UID,              // Property name in API is Uid
                Password = Password,
                Phone = Phone,
                Email = Email,
                EntryDate = EntryDate,
                // 'Activate' field is sent as a string (Y/N) to the controller method
                Activate = IsActive ? "Y" : "N"
            };

            try
            {
                var json = JsonConvert.SerializeObject(userData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("User Information Saved Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Content is read to view error details
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error: {response.StatusCode}\nDetails: {errorMsg}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}