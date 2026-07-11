using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using Newtonsoft.Json;
using Microsoft.Maui.Controls;
using Model_MAUI; // MAUI Commands এবং UI Thread এর জন্য

// আপনার MAUI প্রজেক্টের Namespace অনুযায়ী পরিবর্তন করে নেবেন
namespace ModelView_MAUI
{
    public class DesignationViewModel : INotifyPropertyChanged
    {
        private readonly string _apiUrl = "http://74.208.227.170:8050//api/Data/SaveDesignation";

        // মনে রাখবেন: MAUI প্রজেক্টে Model ক্লাসের নাম এবং প্রপার্টি যেন ঠিক থাকে (যেমন: Responsiblity বানানটি খেয়াল করবেন)
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
                    Designation.Activate = value ? "Y" : "N";
                }
                OnPropertyChanged(nameof(IsActiveChecked));
            }
        }

        public ICommand SaveCommand { get; }

        public DesignationViewModel()
        {
            ResetForm();
            // MAUI-এর বিল্ট-ইন Command ব্যবহার করা হয়েছে
            SaveCommand = new Command(async () => await SaveDesignation());
        }

        private async Task SaveDesignation()
        {
            if (string.IsNullOrWhiteSpace(Designation.DesigCode))
            {
                // MAUI-তে পপ-আপ অ্যালার্ট দেখানোর নিয়ম
                if (Shell.Current != null)
                {
                    await Shell.Current.DisplayAlert("Validation Error", "Please enter a Designation Code.", "OK");
                }
                return;
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Designation.LastUpdate = DateTime.Now;

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
                Responsiblity = string.Empty, // আপনার মডেলের বানান অনুযায়ী
                EntryDate = DateTime.Now,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now, // MAUI DatePicker-এর জন্য Nullable এর পরিবর্তে ডিফল্ট কারেন্ট ডেট দেওয়া নিরাপদ
                EntryByUid = "Admin",
                LastUpdate = DateTime.Now,
                Activate = "N"
            };
            IsActiveChecked = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}