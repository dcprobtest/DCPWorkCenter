namespace View_MAUI;

public partial class US_AccountsLogin : ContentPage
{
    public US_AccountsLogin()
    {
        InitializeComponent();
    }

    private async void btnAccountLogin_Clicked(object sender, EventArgs e)
    {
        string adminId = txtAccountUserID.Text;
        string password = pbAccountPassword.Text;

        if (string.IsNullOrEmpty(password))
        {
            await DisplayAlert("Error", "Please enter your password.", "OK");
            return;
        }

        // Logic for authentication goes here
        await DisplayAlert("Success", "Authentication process initiated.", "OK");
    }
}