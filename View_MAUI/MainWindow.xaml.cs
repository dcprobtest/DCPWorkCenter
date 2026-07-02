using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace View_MAUI
{
    public partial class MainWindow : ContentPage
    {
        private IDispatcherTimer _timer;
        private Border _activeBorder;
        private double _angle = 0;

        // Properties to hold logged-in user details if needed later
        public string LoggedInUserId { get; private set; }
        public string LoggedInUserName { get; private set; }
        public int UserActivateStatus { get; private set; }

        // Parameterized Constructor to accept login data
        public MainWindow(string userId, string userName, int status)
        {
            InitializeComponent();

            LoggedInUserId = userId;
            LoggedInUserName = userName;
            UserActivateStatus = status;

            // Welcome message update based on authenticated user name
            lblTitle.Text = $"Welcome, {LoggedInUserName}";

            // Initialize Animation Timer
            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(30);
            _timer.Tick += (s, e) => UpdateRainbowBorder();
        }

        private async void OnPointerEntered(object sender, PointerEventArgs e)
        {
            if (sender is Border border)
            {
                _activeBorder = border;
                await border.ScaleTo(1.05, 150, Easing.SpringOut);
                border.StrokeThickness = 4;
                _timer.Start();
            }
        }

        private async void OnPointerExited(object sender, PointerEventArgs e)
        {
            if (sender is Border border)
            {
                _timer.Stop();
                _activeBorder = null;
                _angle = 0;

                await border.ScaleTo(1.0, 150, Easing.SpringIn);
                border.Stroke = Colors.Transparent;
                border.StrokeThickness = 0;
            }
        }

        private void UpdateRainbowBorder()
        {
            if (_activeBorder == null) return;

            _angle += 0.1;

            double x1 = 0.5 + Math.Cos(_angle) * 0.5;
            double y1 = 0.5 + Math.Sin(_angle) * 0.5;
            double x2 = 0.5 + Math.Cos(_angle + Math.PI) * 0.5;
            double y2 = 0.5 + Math.Sin(_angle + Math.PI) * 0.5;

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

        // --- Event Handlers for Navigation/Actions ---

        private void btnVideoClass_Tapped(object sender, TappedEventArgs e)
        {
            // Navigate to Video Class page here
        }

        private void btnFileManager_Tapped(object sender, TappedEventArgs e)
        {
            // Navigate to File Manager page here
        }

        private void btnAdminForms_Tapped(object sender, TappedEventArgs e)
        {
            // Navigate to Admin Forms dashboard here
        }

        private void btnClassForms_Tapped(object sender, TappedEventArgs e)
        {
            // Navigate to Class Forms dashboard here
        }

        private void btnAccounts_Tapped(object sender, TappedEventArgs e)
        {
            // Navigate to Accounts Form here
        }

        private void btnUSAccounts_Tapped(object sender, TappedEventArgs e)
        {
            // Navigate to US Accounts login/form here
        }

        private void btnScrum_Tapped(object sender, TappedEventArgs e)
        {
            // Navigate to Scrum section here
        }

        private void btnMessage_Clicked(object sender, EventArgs e)
        {
            // Chat/Notification system click action
        }
    }
}