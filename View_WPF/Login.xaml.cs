using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json; // JsonElement ব্যবহারের জন্য

// কনফ্লিক্ট দূর করার জন্য নিচের Alias গুলো যোগ করা হলো
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        // আপনার API এর সঠিক Base URL এখানে দিন
        private readonly string apiBaseUrl = "https://localhost:7073/";

        public Login()
        {
            InitializeComponent();
        }

        private async void btnAuthenticate_Click(object sender, RoutedEventArgs e)
        {
            // UI থেকে ইনপুট নেওয়া
            string uid = txtUserId.Text.Trim();
            string password = txtPassword.Password.Trim();

            // ভ্যালিডেশন
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter User ID and Password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // বাটন ডিজেবল করা
            btnAuthenticate.IsEnabled = false;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // API এর মডেল অনুযায়ী ডাটা তৈরি
                    var loginRequest = new
                    {
                        Uid = uid,
                        Password = password
                    };

                    // API তে POST রিকোয়েস্ট পাঠানো
                    var response = await client.PostAsJsonAsync($"{apiBaseUrl}api/Data/Login", loginRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        // ১. API রেসপন্স থেকে ডাটা পড়া (যাতে নাম পাওয়া যায়)
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var loginData = JsonSerializer.Deserialize<JsonElement>(jsonString);

                        // আপনার এপিআই যদি অবজেক্ট রিটার্ন করে যাতে 'FName' বা 'Name' আছে
                        string authenticatedUID = txtUserId.Text.Trim();
                        string authenticatedName = "";

                        // API থেকে নাম বের করার চেষ্টা (আপনার API এর প্রপার্টি নাম অনুযায়ী এটি চেক করুন)
                        if (loginData.TryGetProperty("FName", out var nameProp))
                        {
                            authenticatedName = nameProp.GetString();
                        }
                        else if (loginData.TryGetProperty("username", out var userProp))
                        {
                            authenticatedName = userProp.GetString();
                        }

                        // ২. MainWindow ওপেন করার সময় UID এবং Name উভয়ই প্যারামিটার হিসেবে পাঠিয়ে দিন
                        MainWindow mainWin = new MainWindow(authenticatedUID, authenticatedName);
                        mainWin.Show();

                        // বর্তমান লগইন উইন্ডো বন্ধ করা
                        this.Close();
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        MessageBox.Show("Invalid User ID or Password! Please try again.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                        txtPassword.Clear();
                        txtUserId.Focus();
                    }
                    else
                    {
                        MessageBox.Show($"Login failed. Status Code: {response.StatusCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not connect to API: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // বাটন আবার এনাবল করা
                btnAuthenticate.IsEnabled = true;
            }
        }
    }
}