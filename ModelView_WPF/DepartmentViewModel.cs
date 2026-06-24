using ModelView_WPF;
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
        // স্ক্রিনশট অনুযায়ী পোর্ট 7073 এবং রাউট একদম সঠিক আছে।
        private readonly string _apiUrl = "http://74.208.227.170:8050//api/Data/SaveDepartment";

        private DepartmentModel _dept;
        private bool _isActive;

        public DepartmentViewModel()
        {
            ResetForm();
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
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
            // ১. বেসিক ভ্যালিডেশন
            if (Dept == null || string.IsNullOrEmpty(Dept.DeptCode))
            {
                MessageBox.Show("দয়া করে Department Code প্রদান করুন।", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // ২. SSL সার্টিফিকেট জনিত সমস্যা এড়াতে HttpClientHandler ব্যবহার (ঐচ্ছিক কিন্তু নিরাপদ)
                var handler = new HttpClientHandler();
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                using (HttpClient client = new HttpClient(handler))
                {
                    // ৩. JSON এ রূপান্তর
                    var json = JsonConvert.SerializeObject(Dept);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // ৪. API-তে ডাটা পাঠানো
                    HttpResponseMessage response = await client.PostAsync(_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Department সফলভাবে সেভ হয়েছে!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ResetForm();
                    }
                    else
                    {
                        // ৫. NotFound বা অন্য এরর আসলে বিস্তারিত চেক করা
                        string errorDetails = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"API Error ({response.StatusCode}):\nপাথটি পাওয়া যাচ্ছে না বা ডাটা ফরম্যাটে সমস্যা আছে।\n{errorDetails}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"API-র সাথে সংযোগ স্থাপন করা সম্ভব হয়নি।\nপোর্ট 7073 এবং API সচল আছে কিনা চেক করুন।\nবিস্তারিত: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
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