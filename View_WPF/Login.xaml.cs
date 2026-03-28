using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;

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
            // UI থেকে ইনপুট নেওয়া (নিশ্চিত করুন x:Name গুলো ঠিক আছে)
            string uid = txtUserId.Text.Trim();
            string password = txtPassword.Password.Trim();

            // ভ্যালিডেশন
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter User ID and Password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // বাটন ডিজেবল করা (যাতে বারবার ক্লিক না হয়)
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
                    // আপনার কন্ট্রোলারের রুট অনুযায়ী 'api/Data/Login' ব্যবহার করা হয়েছে
                    var response = await client.PostAsJsonAsync($"{apiBaseUrl}api/Data/Login", loginRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        // ১. টেক্সটবক্স থেকে UID-টি নিন
                        string authenticatedUID = txtUserId.Text.Trim();

                        // লগইন সাকসেসফুল মেসেজটি বাদ দেওয়া হয়েছে

                        // ২. MainWindow ওপেন করার সময় UID প্যারামিটার হিসেবে পাঠিয়ে দিন
                        MainWindow mainWin = new MainWindow(authenticatedUID);
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
                // API রান না থাকলে বা নেটওয়ার্ক সমস্যা থাকলে এই এরর দেখাবে
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