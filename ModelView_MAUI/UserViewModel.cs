using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using Microsoft.Maui.Devices; // DeviceInfo এর জন্য

namespace View_MAUI
{
    public class UserViewModel : INotifyPropertyChanged
    {
        // ডাইনামিক URL লজিক
        private string GetBaseUrl() => DeviceInfo.Platform == DevicePlatform.Android
            ? "https://10.0.2.2:7073/"
            : "https://localhost:7073/";

        private string _baseUrl => $"{GetBaseUrl()}api/Data/";

        private readonly HttpClient _httpClient;
        private string _loggedInUid; // UID রিসিভ করার জন্য

        // Properties with OnPropertyChanged for UI binding
        private string _uid;
        public string UID { get => _uid; set { _uid = value; OnPropertyChanged(); } }

        private string _password;
        public string Password { get => _password; set { _password = value; OnPropertyChanged(); } }

        private string _phone;
        public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(); } }

        private string _email;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private bool _isActive;
        public bool IsActive { get => _isActive; set { _isActive = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }

        // কনস্ট্রাক্টর আপডেট করা হয়েছে UID রিসিভ করার জন্য
        public UserViewModel(string uid)
        {
            _loggedInUid = uid;

            // SSL bypass for development
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            _httpClient = new HttpClient(handler);

            SaveCommand = new Command(async () => await ExecuteSave());
        }

        private async Task ExecuteSave()
        {
            var userData = new
            {
                Uid = UID,
                Password = Password,
                Phone = Phone,
                Email = Email,
                EntryDate = DateTime.Now,
                EntryByUid = _loggedInUid, // রিসিভ করা UID ব্যবহার করা হয়েছে
                Activate = IsActive ? "Y" : "N"
            };

            try
            {
                var json = JsonSerializer.Serialize(userData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl + "SaveUser", content);

                if (response.IsSuccessStatusCode)
                {
                    await Shell.Current.DisplayAlert("Success", "User Information Saved Successfully!", "OK");
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    await Shell.Current.DisplayAlert("API Error", $"Status: {response.StatusCode}\nDetails: {errorMsg}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Connection Error", ex.Message, "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}