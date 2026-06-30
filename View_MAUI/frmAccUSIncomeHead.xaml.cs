namespace View_MAUI;

public partial class frmAccUSIncomeHead : ContentPage
{
    public frmAccUSIncomeHead()
    {
        InitializeComponent();
    }

    private async void Exit_Clicked(object sender, EventArgs e)
    {
        
        await Navigation.PopAsync();
    }
}