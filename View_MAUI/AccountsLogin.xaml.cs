using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace View_MAUI
{
    public partial class AccountsLogin : ContentPage
    {
        public AccountsLogin()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Set initial parameters for the animation
            // LoginCard is accessible here directly as it is defined in XAML
            LoginCard.TranslationY = 100;
            LoginCard.Scale = 0.9;
            LoginCard.Opacity = 0;

            // Execute the smooth EaseOut entry animation concurrently
            await Task.WhenAll(
                LoginCard.TranslateTo(0, 0, 800, Easing.CubicOut),
                LoginCard.ScaleTo(1.0, 800, Easing.CubicOut),
                LoginCard.FadeTo(1.0, 600, Easing.CubicOut)
            );
        }

        private void btnAccountLogin_Click(object sender, EventArgs e)
        {
            // Secure access logic goes here
        }
    }
}