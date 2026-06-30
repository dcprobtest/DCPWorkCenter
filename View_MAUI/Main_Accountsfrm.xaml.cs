using Microsoft.Maui.Controls.Shapes;

namespace View_MAUI
{
    public partial class Main_Accountsfrm : ContentPage
    {
        private IDispatcherTimer _timer;
        private Border _activeBorder;
        private double _angle = 0;

        public Main_Accountsfrm()
        {
            InitializeComponent();

            // Timer setup (updates color every 30 milliseconds)
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
                _timer.Start(); // Start timer when mouse enters
            }
        }

        private async void OnPointerExited(object sender, PointerEventArgs e)
        {
            if (sender is Border border)
            {
                _timer.Stop(); // Stop timer when mouse exits
                _activeBorder = null;

                // Reset border rotation and scale
                border.Rotation = 0;
                _angle = 0;

                await border.ScaleTo(1.0, 150, Easing.SpringIn);
                border.Stroke = Colors.Transparent;
                border.StrokeThickness = 0;
            }
        }

        private void UpdateRainbowBorder()
        {
            if (_activeBorder == null) return;

            // Increment angle to calculate new gradient positions
            _angle += 0.1;

            // Calculate coordinates based on the angle to simulate rotating gradient
            double x1 = 0.5 + Math.Cos(_angle) * 0.5;
            double y1 = 0.5 + Math.Sin(_angle) * 0.5;
            double x2 = 0.5 + Math.Cos(_angle + Math.PI) * 0.5;
            double y2 = 0.5 + Math.Sin(_angle + Math.PI) * 0.5;

            // Apply rainbow color loop with rotating Start and End points
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

            // Removed _activeBorder.Rotation = _angle; to keep the button static
        }
    }
}