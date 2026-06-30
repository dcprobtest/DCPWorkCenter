namespace View_MAUI;

public partial class frmAccIncomeHead : ContentPage
{
    public frmAccIncomeHead()
    {
        InitializeComponent();
        
        
        this.Loaded += FrmAccIncomeHead_Loaded;
    }

    private async void FrmAccIncomeHead_Loaded(object sender, EventArgs e)
    {
        TopSection.TranslationY = 30;
        ButtonSection.TranslationY = 30;
        TopSection.Opacity = 0;
        ButtonSection.Opacity = 0;

        await Task.WhenAll(
            TopSection.TranslateTo(0, 0, 600, Easing.CubicOut),
            TopSection.FadeTo(1, 600),
            ButtonSection.TranslateTo(0, 0, 600, Easing.CubicOut),
            ButtonSection.FadeTo(1, 600)
        );
    }

    private void Exit_Clicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }
}