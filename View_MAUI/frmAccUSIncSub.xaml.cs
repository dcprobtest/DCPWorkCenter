namespace View_MAUI;

public partial class frmAccUSIncSub : ContentPage
{
    public frmAccUSIncSub()
    {
        InitializeComponent();
    }

    private async void Exit_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}