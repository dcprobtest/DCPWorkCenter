using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using View_WPF.Model;
using ModelView_WPF;
using Newtonsoft.Json.Serialization; // এটি যোগ করা হয়েছে

namespace View_WPF.ModelView_WPF
{
    public class EmpInfoViewModel : INotifyPropertyChanged
    {
        // API Base URL
        private readonly string _baseUrl = "https://localhost:7073/api/Data/";

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
            Emp.EntryDate = DateTime.Now;
            Emp.StartDate = DateTime.Now;
            Emp.EndDate = DateTime.Now;
            Emp.DOB = DateTime.Now; // DOB এর প্রাথমিক মান সেট করা হয়েছে

            // আলাদা ফাইলে থাকা RelayCommand ব্যবহার করা হচ্ছে
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
        }

        private async Task ExecuteSave()
        {
            // আপনার এরর ফিক্স: Emp.UID নিশ্চিত করা হয়েছে
            if (string.IsNullOrEmpty(Emp.UID))
            {
                MessageBox.Show("Please enter User ID (UID).");
                return;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // তারিখকে yyyy-MM-dd ফরম্যাটে পাঠানোর জন্য সেটিংস
                    // এবং ফিল্ডের নামের বানান (PascalCase to camelCase) ঠিক রাখার জন্য Resolver যোগ করা হয়েছে
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
                        MessageBox.Show("Employee Information Saved Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        ResetForm();
                    }
                    else
                    {
                        string error = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Failed to save. Error: {error}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}