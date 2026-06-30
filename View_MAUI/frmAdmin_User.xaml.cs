namespace View_MAUI;

public partial class frmAdmin_User : ContentPage
{
    IDispatcherTimer timer;
    double offset = 0;

    public frmAdmin_User()
    {
        InitializeComponent();
        StartAnimation();
    }

    private void StartAnimation()
    {
        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(50);
        timer.Tick += (s, e) =>
        {
            offset += 0.01;
            if (offset >= 1.0) offset = 0;

            // ????? ?????? ???????
            var gradient = MainGradientBrush;
            foreach (var stop in gradient.GradientStops)
            {
                stop.Offset = (float)((stop.Offset + 0.01) % 1.0);
            }
        };
        timer.Start();
    }
}