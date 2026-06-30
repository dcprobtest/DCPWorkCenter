using Microsoft.Maui.Controls;

namespace View_MAUI;

public partial class Login : ContentPage
{
    public Login()
    {
        InitializeComponent();
    }

    // Click event handler referenced in the XAML file's Button
    private void btnAuthenticate_Clicked(object sender, EventArgs e)
    {
        string userId = txtUserId.Text;
        string password = txtPassword.Text;

        // Insert your login validation logic here
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
        {
            DisplayAlert("Error", "Please enter both User ID and Password.", "OK");
        }
        else
        {
            // Actions to perform after a successful login
            DisplayAlert("Success", "Authentication successful for user: " + userId, "OK");

            // Example: To navigate to another page:
            // Navigation.PushAsync(new MainPage());
        }
    }
}