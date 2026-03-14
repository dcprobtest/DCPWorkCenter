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
        // আপনার DataController অনুযায়ী সঠিক API Route (api/Data/SaveDesignation)
        private readonly string _apiUrl = "https://localhost:7155/api/Data/SaveDesignation";

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
                // ডাটাবেস ডিজাইন অনুযায়ী 'Y' বা 'N' সেট করা
                if (Designation != null)
                {
                    Designation.Activate = value ? "Y" : "N";
                }
                OnPropertyChanged(nameof(IsActiveChecked));
            }
        }

        public ICommand SaveCommand { get; }

        public DesignationViewModel()
        {
            ResetForm();
            // RelayCommand ফাইল থেকে এটি কল হবে
            SaveCommand = new RelayCommand(async (obj) => await SaveDesignation());
        }

        private async Task SaveDesignation()
        {
            if (string.IsNullOrEmpty(Designation.DesigCode))
            {
                MessageBox.Show("Please enter a Designation Code.");
                return;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(Designation);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // এখানে _apiUrl ব্যবহার করা হয়েছে যা DataController এর সাথে সামঞ্জস্যপূর্ণ
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
                MessageBox.Show("Could not connect to API. Make sure the API is running.\nException: " + ex.Message);
            }
        }

        private void ResetForm()
        {
            Designation = new DesignationModel
            {
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                Activate = "N"
            };
            IsActiveChecked = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}