using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using Model_WPF;
using System.ComponentModel;
using System.Windows;

namespace ModelView_WPF
{
    public class DesignationViewModel : INotifyPropertyChanged
    {
        // আপনার অনুরোধ অনুযায়ী পোর্ট এবং কন্ট্রোলার রুট সেট করা হয়েছে
        private readonly string _apiUrl = "https://localhost:7073/api/Data/SaveDesignation";

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
                    // API লজিক অনুযায়ী 'Y' বা 'N' সেট করা হচ্ছে
                    Designation.Activate = value ? "Y" : "N";
                }
                OnPropertyChanged(nameof(IsActiveChecked));
            }
        }

        public ICommand SaveCommand { get; }

        public DesignationViewModel()
        {
            ResetForm();
            // RelayCommand ব্যবহার করে Save লজিক বাইন্ড করা হয়েছে
            SaveCommand = new RelayCommand(async (obj) => await SaveDesignation());
        }

        private async Task SaveDesignation()
        {
            // ভ্যালিডেশন: DesigCode ছাড়া সেভ হবে না
            if (string.IsNullOrWhiteSpace(Designation.DesigCode))
            {
                MessageBox.Show("Please enter a Designation Code.");
                return;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // API-এর মডেলের সাথে সামঞ্জস্য রাখতে LastUpdate আপডেট করা হচ্ছে
                    Designation.LastUpdate = DateTime.Now;

                    var json = JsonConvert.SerializeObject(Designation);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(_apiUrl, content);

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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect to API at {_apiUrl}. \nException: {ex.Message}");
            }
        }

        private void ResetForm()
        {
            // নতুন এন্ট্রি করার জন্য ফর্ম রিসেট
            Designation = new DesignationModel
            {
                DesigCode = string.Empty,
                Description = string.Empty,
                Responsiblity = string.Empty,
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = null,
                EntryByUid = "Admin", // প্রয়োজন অনুযায়ী ডাইনামিক করতে পারেন
                LastUpdate = DateTime.Now,
                Activate = "N"
            };
            IsActiveChecked = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}