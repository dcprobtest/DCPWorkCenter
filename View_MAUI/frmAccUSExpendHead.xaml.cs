namespace View_MAUI;

public partial class frmAccUSExpendHead : ContentPage
{
    public frmAccUSExpendHead()
    {
        InitializeComponent();
    }

    private async void Exit_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}