using Microsoft.Maui.Controls.Shapes;

namespace View_MAUI;

public partial class Main_frmUSAccounts : ContentPage
{
    private IDispatcherTimer _timer;
    private Border _activeBorder;
    private double _angle = 0;

    public string LoggedInUserId { get; private set; }

    public Main_frmUSAccounts(string userId)
    {
        InitializeComponent();
        LoggedInUserId = userId;

        // Timer setup (updates color every 30 milliseconds)
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(30);
        _timer.Tick += (s, e) => UpdateRainbowBorder();
    }

    // Pointer Entered: Scale up the card and start the rainbow animation
    private async void OnPointerEntered(object sender, PointerEventArgs e)
    {
        if (sender is Border border)
        {
            _activeBorder = border;

            // Trigger scaling animation
            await border.ScaleTo(1.05, 150, Easing.SpringOut);

            border.StrokeThickness = 4;
            _timer.Start(); // Start the timer for border color rotation
        }
    }

    // Pointer Exited: Scale down the card and stop the animation
    private void OnPointerExited(object sender, PointerEventArgs e)
    {
        if (sender is Border border)
        {
            _timer.Stop(); // Stop the animation timer
            _activeBorder = null;

            // Reset rotation angle
            _angle = 0;

            // Reset card size and appearance
            border.ScaleTo(1.0, 150, Easing.SpringIn);
            border.Stroke = Colors.Transparent;
            border.StrokeThickness = 0;
        }
    }

    // Update the rainbow border color dynamically based on angle
    private void UpdateRainbowBorder()
    {
        if (_activeBorder == null) return;

        // Increment the angle for rotation effect
        _angle += 0.1;

        // Calculate gradient start and end points based on the angle
        double x1 = 0.5 + Math.Cos(_angle) * 0.5;
        double y1 = 0.5 + Math.Sin(_angle) * 0.5;
        double x2 = 0.5 + Math.Cos(_angle + Math.PI) * 0.5;
        double y2 = 0.5 + Math.Sin(_angle + Math.PI) * 0.5;

        // Apply dynamic LinearGradientBrush to the active border's stroke
        _activeBorder.Stroke = new LinearGradientBrush(
            new GradientStopCollection {
                new GradientStop(Colors.Red, 0.0f),
                new GradientStop(Colors.Orange, 0.2f),
                new GradientStop(Colors.Yellow, 0.4f),
                new GradientStop(Colors.Green, 0.6f),
                new GradientStop(Colors.Blue, 0.8f),
                new GradientStop(Colors.Purple, 1.0f)
            },
            new Point(x1, y1),
            new Point(x2, y2)
        );
    }

    // XAML ? ????? ??? TapGestureRecognizer ?? ???? ?????? ??????????
    private async void OnCardTapped(object sender, TappedEventArgs e)
    {
        string parameter = e.Parameter?.ToString();

        switch (parameter)
        {
            case "Acc_US_Exp_Bub":
                await Navigation.PushAsync(new frmAccUSExpBub(LoggedInUserId));
                break;
            case "Acc_US_Expend_Head":
                await Navigation.PushAsync(new frmAccUSExpendHead(LoggedInUserId));
                break;
            case "Acc_US_Income_Head":
                await Navigation.PushAsync(new frmAccUSIncomeHead(LoggedInUserId));
                break;
            case "Acc_US_Inc_Sub":
                await Navigation.PushAsync(new frmAccUSIncSub(LoggedInUserId));
                break;
            default:
                break;
        }
    }

    // Navigation handlers for buttons (Keep as is)
    private async void btnAccUSExpBub_Click(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new frmAccUSExpBub(LoggedInUserId));
    }

    private async void btnAccUSExpendHead_Click(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new frmAccUSExpendHead(LoggedInUserId));
    }

    private async void btnAccUSIncomeHead_Click(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new frmAccUSIncomeHead(LoggedInUserId));
    }

    private async void btnAccUSIncSub_Click(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new frmAccUSIncSub(LoggedInUserId));
    }
}