using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace View_MAUI
{
    public partial class US_AccountsLogin : ContentPage
    {
        private bool isAnimationRunning = false;
        public string LoggedInUserId { get; private set; }

        public US_AccountsLogin(string userId)
        {
            InitializeComponent();
            LoggedInUserId = userId;

            // Setting the User ID to the entry field
            txtAccountUserID.Text = LoggedInUserId;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Find references at runtime
            var loginCard = this.FindByName<Border>("LoginCard");
            var rotatingRainbow = this.FindByName<BoxView>("rainbowRotatingBorder");

            // Setup animation
            if (loginCard != null)
            {
                loginCard.TranslationY = 100;
                loginCard.Opacity = 0;

                // Staggered Slide-up effect
                _ = loginCard.TranslateTo(0, 0, 800, Easing.CubicOut);
                await loginCard.FadeTo(1.0, 600, Easing.CubicOut);
            }

            // Start rainbow border animation
            if (!isAnimationRunning && rotatingRainbow != null)
            {
                isAnimationRunning = true;
                StartRainbowAnimation(rotatingRainbow);
            }
        }

        private async void StartRainbowAnimation(BoxView rotatingRainbow)
        {
            // Loop for smooth rotation of the rainbow background
            while (isAnimationRunning && rotatingRainbow != null)
            {
                await rotatingRainbow.RelRotateTo(360, 4000, Easing.Linear);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Stop animation when page disappears to prevent memory leaks
            isAnimationRunning = false;
        }

        private async void btnAccountLogin_Clicked(object sender, EventArgs e)
        {
            // Add password validation logic here
            string password = pbAccountPassword.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(password))
            {
                await DisplayAlert("Validation Error", "Please enter your password.", "OK");
                return;
            }

            // Navigate to the next form upon successful authentication
            isAnimationRunning = false;
            await Navigation.PushAsync(new Main_frmUSAccounts(LoggedInUserId));
        }
    }
}