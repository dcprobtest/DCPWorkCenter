using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace View_MAUI
{
    public partial class Main_Adminfrm : ContentPage
    {
        private IDispatcherTimer _timer;
        private Border _activeBorder;
        private double _angle = 0;

        // Property to store the received user ID
        public string AdminUserId { get; private set; }

        // Updated Constructor to accept uid
        public Main_Adminfrm(string uid)
        {
            InitializeComponent();

            // Assign the received uid
            AdminUserId = uid;

            // Timer setup (updates color every 30 milliseconds)
            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(30);
            _timer.Tick += (s, e) => UpdateRainbowBorder();
        }

        // Pointer Entered: Triggered when the pointer enters the border
        private async void OnPointerEntered(object sender, PointerEventArgs e)
        {
            if (sender is Border border)
            {
                _activeBorder = border;

                // Scale up the border
                await border.ScaleTo(1.05, 150, Easing.SpringOut);

                border.StrokeThickness = 4;
                _timer.Start(); // Start the animation timer
            }
        }

        // Pointer Exited: Triggered when the pointer leaves the border
        private async void OnPointerExited(object sender, PointerEventArgs e)
        {
            if (sender is Border border)
            {
                _timer.Stop(); // Stop the animation timer
                _activeBorder = null;

                // Reset angle
                _angle = 0;

                await border.ScaleTo(1.0, 150, Easing.SpringIn);
                border.Stroke = Colors.Transparent;
                border.StrokeThickness = 0;
            }
        }

        // Update the border color dynamically
        private void UpdateRainbowBorder()
        {
            if (_activeBorder == null) return;

            // Increment angle to create the rotating effect
            _angle += 0.1;

            // Calculate gradient coordinates based on the angle
            double x1 = 0.5 + Math.Cos(_angle) * 0.5;
            double y1 = 0.5 + Math.Sin(_angle) * 0.5;
            double x2 = 0.5 + Math.Cos(_angle + Math.PI) * 0.5;
            double y2 = 0.5 + Math.Sin(_angle + Math.PI) * 0.5;

            // Apply the linear gradient brush to the border stroke
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
    }
}