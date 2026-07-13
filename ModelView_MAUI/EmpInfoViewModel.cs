using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices; // Added for DeviceInfo
using View_MAUI.Model;

namespace View_MAUI.ViewModel_MAUI
{
    public class EmpInfoViewModel : INotifyPropertyChanged
    {
        // Dynamic URL logic based on platform
        private string GetBaseUrl() => DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7073/"
            : "https://localhost:7073/";

        private string _baseUrl => $"{GetBaseUrl()}api/Data/";

        private EmpInfoModel _emp = new EmpInfoModel();
        public EmpInfoModel Emp
        {
            get => _emp;
            set
            {
                _emp = value;
                OnPropertyChanged(nameof(Emp));
            }
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
                if (Emp != null)
                {
                    Emp.Activate = value ? "Y" : "N";
                }
            }
        }

        public ICommand SaveCommand { get; }

        public EmpInfoViewModel()
        {
            ResetForm();
            SaveCommand = new Command(async () => await ExecuteSave());
        }

        private async Task ExecuteSave()
        {
            if (string.IsNullOrEmpty(Emp.UID))
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Validation Error", "Please enter User ID (UID).", "OK");
                }
                return;
            }

            try
            {
                // Added handler to bypass SSL for development
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                using (HttpClient client = new HttpClient(handler))
                {
                    var settings = new JsonSerializerSettings
                    {
                        DateFormatString = "yyyy-MM-dd",
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    var json = JsonConvert.SerializeObject(Emp, settings);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(_baseUrl + "SaveEmployee", content);

                    if (response.IsSuccessStatusCode)
                    {
                        if (Shell.Current != null)
                        {
                            await Shell.Current.DisplayAlert("Success", "Employee Information Saved Successfully!", "OK");
                        }
                        ResetForm();
                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                        if (Shell.Current != null)
                        {
                            await Shell.Current.DisplayAlert("API Error", $"Failed to save. Error: {error}", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Error", "An error occurred: " + ex.Message, "OK");
                }
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
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}