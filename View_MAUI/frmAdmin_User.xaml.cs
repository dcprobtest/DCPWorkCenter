using View_MAUI.ViewModel_MAUI;

namespace View_MAUI;

public partial class frmAdmin_User : ContentPage
{
    IDispatcherTimer timer;
    double offset = 0;

    // Property to store the received user ID
    public string AdminUserId { get; private set; }

    public frmAdmin_User(string uid)
    {
        InitializeComponent();

        // Assign the received uid
        AdminUserId = uid;

        // Initialize the ViewModel and pass the uid to it
        this.BindingContext = new UserViewModel(AdminUserId);

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

            // Update gradient offsets
            var gradient = MainGradientBrush;
            foreach (var stop in gradient.GradientStops)
            {
                stop.Offset = (float)((stop.Offset + 0.01) % 1.0);
            }
        };
        timer.Start();
    }
}