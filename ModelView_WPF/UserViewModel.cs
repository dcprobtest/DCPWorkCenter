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
        private readonly string _baseUrl = "https://localhost:7169/api/User/";

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
            // আপনার আলাদা ফাইলে থাকা RelayCommand ব্যবহার করা হয়েছে
            SaveCommand = new RelayCommand(async (obj) => await ExecuteSave());
        }

        private async Task ExecuteSave()
        {
            var userData = new
            {
                UID,
                Password,
                Phone,
                Email,
                EntryDate,
                StartDate,
                EndDate,
                Activate = IsActive ? "Y" : "N"
            };

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var json = JsonConvert.SerializeObject(userData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(_baseUrl + "SaveUser", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("User Information Saved Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        // প্রয়োজনে এখানে ResetForm() মেথড কল করতে পারেন
                    }
                    else
                    {
                        MessageBox.Show("Error: " + response.ReasonPhrase, "API Error", MessageBoxButton.OK, MessageBoxImage.Error);
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