namespace View_MAUI;

public partial class frmAccIncSub : ContentPage
{
    public frmAccIncSub()
    {
        InitializeComponent();
    }

    private async void Exit_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}