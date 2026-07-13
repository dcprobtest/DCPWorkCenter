using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using View_WPF.Model_MAUI;

namespace View_MAUI.ViewModel_MAUI
{
    public class DepartmentViewModel : INotifyPropertyChanged
    {
        // Set the base URL dynamically for Android Emulator or local development
        private string GetBaseUrl() => DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7073/"
            : "https://localhost:7073/";

        private string _apiUrl => $"{GetBaseUrl()}api/Data/SaveDepartment";

        private DepartmentModel _dept;
        private bool _isActive;
        private string _loggedInUid;

        // Constructor modified to accept the uid
        public DepartmentViewModel(string uid)
        {
            _loggedInUid = uid;
            ResetForm();
            SaveCommand = new Command(async () => await ExecuteSave());
        }

        public DepartmentModel Dept
        {
            get => _dept;
            set { _dept = value; OnPropertyChanged(); }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if (Dept != null) Dept.Activate = value ? "Y" : "N";
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }

        private async Task ExecuteSave()
        {
            // 1. Basic validation check
            if (Dept == null || string.IsNullOrEmpty(Dept.DeptCode))
            {
                await Shell.Current.DisplayAlert("Validation Error", "Please provide the Department Code.", "OK");
                return;
            }

            try
            {
                // 2. SSL certificate handler
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                using (HttpClient client = new HttpClient(handler))
                {
                    // 3. Serialize object to JSON
                    var json = JsonConvert.SerializeObject(Dept);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // 4. Send data to API
                    HttpResponseMessage response = await client.PostAsync(_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        await Shell.Current.DisplayAlert("Success", "Department saved successfully!", "OK");
                        ResetForm();
                    }
                    else
                    {
                        // 5. Read error details
                        string errorDetails = await response.Content.ReadAsStringAsync();
                        await Shell.Current.DisplayAlert("Error", $"API Error ({response.StatusCode}):\nPath might be incorrect or data format issue.\n{errorDetails}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Connection Error", $"Could not connect to the API.\nPlease check if the port and API are running.\nDetails: {ex.Message}", "OK");
            }
        }

        private void ResetForm()
        {
            Dept = new DepartmentModel
            {
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                Activate = "N"
            };
            IsActive = false;

            // Refresh all fields by triggering OnPropertyChanged with string.Empty
            OnPropertyChanged(string.Empty);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}