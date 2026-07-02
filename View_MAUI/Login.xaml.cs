using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Maui.Controls;

namespace View_MAUI
{
    public partial class Login : ContentPage
    {
        private readonly string apiBaseUrl = "https://localhost:7073/";
        private bool isAnimationRunning = false;

        public Login()
        {
            InitializeComponent();
        }

        /// <summary>
        /// পেজটি স্ক্রিনে সম্পূর্ণ দৃশ্যমান হওয়ার সাথে সাথে অ্যানিমেশন এবং কার্সার ফোকাস সেট করার জন্য
        /// </summary>
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // রানটাইমে নিরাপদ উপায়ে অবজেক্টের রেফারেন্স খোঁজা
            var title = this.FindByName<Label>("lblTitle");
            var subtitle = this.FindByName<Label>("lblSubtitle");
            var userGrid = this.FindByName<Grid>("gridUser");
            var passGrid = this.FindByName<Grid>("gridPass");
            var btnContainer = this.FindByName<Grid>("btnAuthenticateContainer");
            var rotatingRainbow = this.FindByName<BoxView>("rainbowRotatingBorder");
            var userIdInput = this.FindByName<Entry>("txtUserId");

            // ১. ইউজার আইডি ইনপুট বক্সে কার্সার পাঠিয়ে দেওয়া
            userIdInput?.Focus();

            // ২. WPF এর মতো সুন্দর স্ট্যাগার্ড (Staggered Slide-up) অ্যানিমেশন রান করা
            try
            {
                if (title != null) _ = title.TranslateTo(0, 0, 600, Easing.CubicOut);
                await Task.Delay(80);

                if (subtitle != null) _ = subtitle.TranslateTo(0, 0, 600, Easing.CubicOut);
                await Task.Delay(80);

                if (userGrid != null) _ = userGrid.TranslateTo(0, 0, 600, Easing.CubicOut);
                await Task.Delay(80);

                if (passGrid != null) _ = passGrid.TranslateTo(0, 0, 600, Easing.CubicOut);
                await Task.Delay(80);

                if (btnContainer != null) await btnContainer.TranslateTo(0, 0, 600, Easing.CubicOut);

                // ৩. মাস্কড রেইনবো বক্সভিউ ঘোরানোর মেথড কল করা
                if (!isAnimationRunning && rotatingRainbow != null)
                {
                    isAnimationRunning = true;
                    StartRainbowAnimation(rotatingRainbow);
                }
            }
            catch (Exception)
            {
                // কোনো কারণে অ্যানিমেশন ব্যাহত হলেও মেইন প্রসেস যেন না আটকায়
            }
        }

        /// <summary>
        /// ব্যাকগ্রাউন্ড রেইনবো কালারকে স্মুথলি ঘোরানোর লুপ
        /// </summary>
        private async void StartRainbowAnimation(BoxView rotatingRainbow)
        {
            while (isAnimationRunning && rotatingRainbow != null)
            {
                await rotatingRainbow.RelRotateTo(360, 4000, Easing.Linear);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            isAnimationRunning = false;
        }

        private async void btnAuthenticate_Clicked(object sender, EventArgs e)
        {
            var userIdInput = this.FindByName<Entry>("txtUserId");
            var passwordInput = this.FindByName<Entry>("txtPassword");
            var authButton = this.FindByName<Button>("btnAuthenticate");

            // Taking input from UI safely
            string uid = userIdInput?.Text?.Trim() ?? string.Empty;
            string password = passwordInput?.Text?.Trim() ?? string.Empty;

            // Validation
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Validation Error", "Please enter User ID and Password.", "OK");
                return;
            }

            if (authButton != null) authButton.IsEnabled = false;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var loginRequest = new { Uid = uid, Password = password };
                    var response = await client.PostAsJsonAsync($"{apiBaseUrl}api/Data/Login", loginRequest);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var loginData = JsonSerializer.Deserialize<JsonElement>(jsonString, options);

                        string authenticatedUID = uid;
                        string authenticatedName = string.Empty;
                        int activateStatus = 0;

                        if (loginData.TryGetProperty("fname", out var nameProp) || loginData.TryGetProperty("FName", out nameProp))
                        {
                            authenticatedName = nameProp.GetString() ?? string.Empty;
                        }
                        else if (loginData.TryGetProperty("username", out var userProp))
                        {
                            authenticatedName = userProp.GetString() ?? string.Empty;
                        }

                        if (loginData.TryGetProperty("activate", out var activateProp) || loginData.TryGetProperty("Activate", out activateProp))
                        {
                            if (activateProp.ValueKind == JsonValueKind.Number)
                            {
                                activateStatus = activateProp.GetInt32();
                            }
                            else if (activateProp.ValueKind == JsonValueKind.String)
                            {
                                int.TryParse(activateProp.GetString(), out activateStatus);
                            }
                        }

                        if (activateStatus == 0)
                        {
                            await DisplayAlert("Access Denied", "Your account is deactivated (Status 0). Please contact the Administrator.", "OK");
                        }
                        else if (activateStatus == 1 || activateStatus == 3)
                        {
                            isAnimationRunning = false;

                            if (Application.Current != null)
                            {
                                Application.Current.MainPage = new NavigationPage(new MainWindow(authenticatedUID, authenticatedName, activateStatus));
                            }
                        }
                        else
                        {
                            await DisplayAlert("Access Denied", "You do not have permission to access this system. Only Status 1 and 3 are allowed.", "OK");
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        await DisplayAlert("Login Failed", "Invalid User ID or Password! Please try again.", "OK");
                        if (passwordInput != null) passwordInput.Text = string.Empty;
                        userIdInput?.Focus();
                    }
                    else
                    {
                        await DisplayAlert("Error", $"Login failed. Status Code: {response.StatusCode}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Connection Error", $"Could not connect to API: {ex.Message}", "OK");
            }
            finally
            {
                if (authButton != null) authButton.IsEnabled = true;
            }
        }
    }
}