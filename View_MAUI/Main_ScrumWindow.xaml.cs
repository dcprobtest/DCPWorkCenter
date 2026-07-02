using Microsoft.Maui.Controls.Shapes;

namespace View_MAUI
{
    public partial class Main_ScrumWindow : ContentPage
    {
        private IDispatcherTimer _timer;
        private Border _activeBorder;
        private double _angle = 0;

        public Main_ScrumWindow()
        {
            InitializeComponent();

            // Timer setup (updates color every 30 milliseconds)
            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(30);
            _timer.Tick += (s, e) => UpdateRainbowBorder();
        }

        // Pointer Entered: Starts the animation and timer when the mouse enters the card
        private async void OnPointerEntered(object sender, PointerEventArgs e)
        {
            if (sender is Border border)
            {
                _activeBorder = border;

                // Scale animation
                await border.ScaleTo(1.05, 150, Easing.SpringOut);

                border.StrokeThickness = 4;
                _timer.Start(); // Start timer
            }
        }

        // Pointer Exited: Stops the timer and resets the border when the mouse leaves
        private async void OnPointerExited(object sender, PointerEventArgs e)
        {
            if (sender is Border border)
            {
                _timer.Stop(); // Stop timer
                _activeBorder = null;

                // Reset angle
                _angle = 0;

                await border.ScaleTo(1.0, 150, Easing.SpringIn);
                border.Stroke = Colors.Transparent;
                border.StrokeThickness = 0;
            }
        }

        // Rainbow color update logic
        private void UpdateRainbowBorder()
        {
            if (_activeBorder == null) return;

            // Update angle
            _angle += 0.1;

            // Coordinate calculation for the linear gradient
            double x1 = 0.5 + Math.Cos(_angle) * 0.5;
            double y1 = 0.5 + Math.Sin(_angle) * 0.5;
            double x2 = 0.5 + Math.Cos(_angle + Math.PI) * 0.5;
            double y2 = 0.5 + Math.Sin(_angle + Math.PI) * 0.5;

            // Set color to the border
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

        // Click event handlers
        private void btnAssignmentScrum_Clicked(object sender, EventArgs e)
        {
            // Navigation logic here
        }

        private void btnAssignTechInfo_Clicked(object sender, EventArgs e) { }
        private void btnScrumInfo_Clicked(object sender, EventArgs e) { }
        private void btnTechInfo_Clicked(object sender, EventArgs e) { }
    }
}