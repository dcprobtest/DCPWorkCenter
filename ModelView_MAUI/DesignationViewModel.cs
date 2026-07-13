using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices; // Required for DeviceInfo
using Model_MAUI;

namespace View_MAUI
{
    public class DesignationViewModel : INotifyPropertyChanged
    {
        // Set the base URL dynamically for Android Emulator or local development
        private string GetBaseUrl() => DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7073/"
            : "https://localhost:7073/";

        private string _apiUrl => $"{GetBaseUrl()}api/Data/SaveDesignation";

        private DesignationModel _designation = new DesignationModel();
        private string _loggedInUid;

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
                    Designation.Activate = value ? "Y" : "N";
                }
                OnPropertyChanged(nameof(IsActiveChecked));
            }
        }

        public ICommand SaveCommand { get; }

        // Constructor modified to accept the uid
        public DesignationViewModel(string uid)
        {
            _loggedInUid = uid;
            ResetForm();
            SaveCommand = new Command(async () => await SaveDesignation());
        }

        private async Task SaveDesignation()
        {
            if (string.IsNullOrWhiteSpace(Designation.DesigCode))
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Validation Error", "Please enter a Designation Code.", "OK");
                }
                return;
            }

            try
            {
                // Added SSL certificate bypass for development
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                using (HttpClient client = new HttpClient(handler))
                {
                    Designation.LastUpdate = DateTime.Now;
                    Designation.EntryByUid = _loggedInUid; // Using the passed uid

                    var json = JsonConvert.SerializeObject(Designation);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        if (Shell.Current != null)
                        {
                            await Shell.Current.DisplayAlert("Success", "Designation Saved Successfully!", "OK");
                        }
                        ResetForm();
                    }
                    else
                    {
                        string errorMsg = await response.Content.ReadAsStringAsync();
                        if (Shell.Current != null)
                        {
                            await Shell.Current.DisplayAlert("Error", $"Error: {response.StatusCode}\nDetails: {errorMsg}", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Connection Error", $"Could not connect to API at {_apiUrl}. \nException: {ex.Message}", "OK");
                }
            }
        }

        private void ResetForm()
        {
            Designation = new DesignationModel
            {
                DesigCode = string.Empty,
                Description = string.Empty,
                Responsiblity = string.Empty,
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                EntryByUid = _loggedInUid, // Default to logged-in user
                LastUpdate = DateTime.Now,
                Activate = "N"
            };
            IsActiveChecked = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}