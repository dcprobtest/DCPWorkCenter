using Microsoft.Maui.Controls.Shapes;

namespace View_MAUI;

public partial class frm_STEM : ContentPage
{
    public frm_STEM()
    {
        InitializeComponent();
        this.Loaded += (s, e) => StartMainBorderAnimation();
    }

    // Animation for the main border to rotate the rainbow colors
    private void StartMainBorderAnimation()
    {
        // Check if MainBorder exists and has a LinearGradientBrush
        if (MainBorder != null && MainBorder.Stroke is LinearGradientBrush gradient)
        {
            var rotation = new Animation(v =>
            {
                gradient.StartPoint = new Point(0.5 + 0.5 * Math.Cos(v), 0.5 + 0.5 * Math.Sin(v));
                gradient.EndPoint = new Point(0.5 - 0.5 * Math.Cos(v), 0.5 - 0.5 * Math.Sin(v));
            }, 0, 2 * Math.PI);

            rotation.Commit(MainBorder, "MainBorderAnim", 16, 5000, Easing.Linear, null, () => true);
        }
    }

    // Event handler for the Exit button
    private void btnExit_Clicked(object sender, EventArgs e)
    {
        // Closes the current page
        Navigation.PopAsync();
    }

    // You can add your command logic here or in your ViewModel
    public void StartButtonAnimation(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.Background is LinearGradientBrush gradient)
        {
            var rotation = new Animation(v =>
            {
                gradient.StartPoint = new Point(0.5 + 0.5 * Math.Cos(v), 0.5 + 0.5 * Math.Sin(v));
                gradient.EndPoint = new Point(0.5 - 0.5 * Math.Cos(v), 0.5 - 0.5 * Math.Sin(v));
            }, 0, 2 * Math.PI);

            rotation.Commit(btn, "BtnAnim", 16, 2000, Easing.Linear, null, () => true);
        }
    }

    public void StopButtonAnimation(object sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            btn.AbortAnimation("BtnAnim");
        }
    }
}