using ModelView_WPF;
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
        // 'User' পরিবর্তন করে 'Data' করা হলো
        private readonly string _baseUrl = "https://localhost:7073/api/Data/";

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
            // আপনার আলাদা ফাইলে থাকা RelayCommand ব্যবহার করা হয়েছে
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
        }

        private async Task ExecuteSave()
        {
            // API-এর TblUser মডেলের প্রপার্টি নামের সাথে মিল রেখে অবজেক্ট তৈরি করা হলো
            var userData = new
            {
                Uid = UID,              // API তে প্রপার্টি নাম Uid
                Password = Password,
                Phone = Phone,
                Email = Email,
                EntryDate = EntryDate,
                // কন্ট্রোলার মেথডে 'Activate' ফিল্ডটি string হিসেবে (Y/N) পাঠানো হচ্ছে
                Activate = IsActive ? "Y" : "N"
            };

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(userData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // সঠিক URL: api/Data/SaveUser
                    var response = await client.PostAsync(_baseUrl + "SaveUser", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("User Information Saved Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // এরর ডিটেইলস দেখার জন্য Content রিড করা হচ্ছে
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error: {response.StatusCode}\nDetails: {errorMsg}", "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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