using Microsoft.Maui.Controls.Shapes;

namespace View_MAUI;

public partial class frm_STEM : ContentPage
{
    public frm_STEM()
    {
        InitializeComponent();
        this.Loaded += (s, e) => StartMainBorderAnimation();
    }

    private void StartMainBorderAnimation()
    {
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

    private void btnExit_Clicked(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    public void StartButtonAnimation(object sender, EventArgs e)
    {
        if (sender is Border border)
        {
            var gradient = new LinearGradientBrush();
            gradient.GradientStops.Add(new GradientStop(Colors.Red, 0.0f));
            gradient.GradientStops.Add(new GradientStop(Colors.Yellow, 0.2f));
            gradient.GradientStops.Add(new GradientStop(Colors.Green, 0.4f));
            gradient.GradientStops.Add(new GradientStop(Colors.Cyan, 0.6f));
            gradient.GradientStops.Add(new GradientStop(Colors.Blue, 0.8f));
            gradient.GradientStops.Add(new GradientStop(Colors.Magenta, 1.0f));

            border.Stroke = gradient;
            border.ScaleTo(1.1, 100);

            var rotation = new Animation(v =>
            {
                gradient.StartPoint = new Point(0.5 + 0.5 * Math.Cos(v), 0.5 + 0.5 * Math.Sin(v));
                gradient.EndPoint = new Point(0.5 - 0.5 * Math.Cos(v), 0.5 - 0.5 * Math.Sin(v));
            }, 0, 2 * Math.PI);

            rotation.Commit(border, "BtnAnim", 16, 2000, Easing.Linear, null, () => true);
        }
    }

    public void StopButtonAnimation(object sender, EventArgs e)
    {
        if (sender is Border border)
        {
            border.ScaleTo(1.0, 100);
            border.AbortAnimation("BtnAnim");
            border.Stroke = Colors.Transparent;
        }
    }
}