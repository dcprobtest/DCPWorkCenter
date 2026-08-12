using ModelView_WPF;
using ModelView_WPF.Helpers;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using View_WPF.Model_WPF;

namespace View_WPF.ModelView_WPF
{
    public class DepartmentViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private string _uid;

        private DepartmentModel _dept;
        private bool _isActive;

        public DepartmentViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            ResetForm();
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
        }

        public DepartmentViewModel(string uid) : this()
        {
            _uid = uid;
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
            if (Dept == null || string.IsNullOrEmpty(Dept.DeptCode))
            {
                MessageBox.Show("Please provide a Department Code.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (!string.IsNullOrEmpty(_uid))
                {
                    Dept.EntryByUID = _uid;
                }

                var json = JsonConvert.SerializeObject(Dept);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync(ApiSettings.SaveDepartmentUrl(), content);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Department saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ResetForm();
                }
                else
                {
                    string errorDetails = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"API Error ({response.StatusCode}):\nPath not found or data format issue.\n{errorDetails}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to establish connection with the API.\nPlease check if the API is running.\nDetails: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetForm()
        {
            Dept = new DepartmentModel
            {
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                Activate = "N"
            };
            IsActive = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}