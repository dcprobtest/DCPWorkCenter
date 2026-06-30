namespace View_MAUI;

public partial class frmAccUSExpBub : ContentPage
{
    public frmAccUSExpBub()
    {
        InitializeComponent();
    }

    private async void Exit_Clicked(object sender, EventArgs e)
    {
        
        await Navigation.PopAsync();
    }
}