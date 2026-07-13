using System;
using Microsoft.Maui.Controls;

namespace View_MAUI
{
    public partial class MainWindow : ContentPage
    {
        private IDispatcherTimer _timer;
        private Border _activeBorder;
        private double _angle = 0;

        public string LoggedInUserId { get; private set; }
        public string LoggedInUserName { get; private set; }
        public int UserActivateStatus { get; private set; }

        public MainWindow(string userId, string userName, int status)
        {
            InitializeComponent();

            LoggedInUserId = userId;
            LoggedInUserName = userName;
            UserActivateStatus = status;

            // Setup animation rotation speed timer
            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(25);
            _timer.Tick += (s, e) => UpdateRainbowBorder();

            // Attach smooth hover to cards after page loads
            Loaded += (s, e) => AttachHoverToAllCards();
        }

        private void AttachHoverToAllCards()
        {
            if (Content is Grid mainGrid && mainGrid.Children[2] is Grid contentGrid)
            {
                foreach (var rowChild in contentGrid.Children)
                {
                    if (rowChild is Grid innerGrid)
                    {
                        foreach (var card in innerGrid.Children)
                        {
                            if (card is Border border && border.BackgroundColor != Colors.Transparent)
                            {
                                var pointerGesture = new PointerGestureRecognizer();
                                pointerGesture.PointerEntered += OnPointerEntered;
                                pointerGesture.PointerExited += OnPointerExited;
                                border.GestureRecognizers.Add(pointerGesture);
                            }
                        }
                    }
                }
            }
        }

        private async void OnPointerEntered(object sender, PointerEventArgs e)
        {
            if (sender is Border border)
            {
                _activeBorder = border;
                border.StrokeThickness = 3.5;
                _timer.Start();
                // Futuristic hover scale effect
                await border.ScaleTo(1.05, 120, Easing.CubicOut);
            }
        }

        private async void OnPointerExited(object sender, PointerEventArgs e)
        {
            if (sender is Border border)
            {
                if (_activeBorder == border)
                {
                    _timer.Stop();
                    _activeBorder = null;
                }
                border.StrokeThickness = 0;
                border.Stroke = Colors.Transparent;
                await border.ScaleTo(1.0, 120, Easing.CubicIn);
            }
        }

        private void UpdateRainbowBorder()
        {
            if (_activeBorder == null) return;

            _angle += 0.08;

            double x1 = 0.5 + Math.Cos(_angle) * 0.5;
            double y1 = 0.5 + Math.Sin(_angle) * 0.5;
            double x2 = 0.5 + Math.Cos(_angle + Math.PI) * 0.5;
            double y2 = 0.5 + Math.Sin(_angle + Math.PI) * 0.5;

            _activeBorder.Stroke = new LinearGradientBrush(
                new GradientStopCollection {
                    new GradientStop(Color.FromHex("#FF0000"), 0.0f),
                    new GradientStop(Color.FromHex("#FFFF00"), 0.2f),
                    new GradientStop(Color.FromHex("#00FF00"), 0.4f),
                    new GradientStop(Color.FromHex("#00FFFF"), 0.6f),
                    new GradientStop(Color.FromHex("#0000FF"), 0.8f),
                    new GradientStop(Color.FromHex("#FF00FF"), 1.0f)
                },
                new Point(x1, y1),
                new Point(x2, y2)
            );
        }

        // --- Safety Navigation Event Handlers ---
        private async void btnVideoClass_Tapped(object sender, TappedEventArgs e)
        {
            try
            {
                if (Navigation != null)
                    await Navigation.PushAsync(new frmClass_ClassPlayer(LoggedInUserId));
                else
                    Application.Current.MainPage = new NavigationPage(new frmClass_ClassPlayer(LoggedInUserId));
            }
            catch (Exception ex) { await DisplayAlert("Navigation Error", ex.Message, "OK"); }
        }

        private async void btnAdminForms_Tapped(object sender, TappedEventArgs e)
        {
            try
            {
                if (Navigation != null)
                    await Navigation.PushAsync(new Main_Adminfrm(LoggedInUserId));
                else
                    Application.Current.MainPage = new NavigationPage(new Main_Adminfrm(LoggedInUserId));
            }
            catch (Exception ex) { await DisplayAlert("Navigation Error", ex.Message, "OK"); }
        }

        private async void btnClassForms_Tapped(object sender, TappedEventArgs e)
        {
            try
            {
                if (Navigation != null)
                    await Navigation.PushAsync(new Main_Classfrm(LoggedInUserId));
                else
                    Application.Current.MainPage = new NavigationPage(new Main_Classfrm(LoggedInUserId));
            }
            catch (Exception ex) { await DisplayAlert("Navigation Error", ex.Message, "OK"); }
        }

        private async void btnAccounts_Tapped(object sender, TappedEventArgs e)
        {
            try
            {
                if (Navigation != null)
                    await Navigation.PushAsync(new AccountsLogin(LoggedInUserId));
                else
                    Application.Current.MainPage = new NavigationPage(new AccountsLogin(LoggedInUserId));
            }
            catch (Exception ex) { await DisplayAlert("Navigation Error", ex.Message, "OK"); }
        }

        private async void btnUSAccounts_Tapped(object sender, TappedEventArgs e)
        {
            try
            {
                if (Navigation != null)
                    await Navigation.PushAsync(new US_AccountsLogin(LoggedInUserId));
                else
                    Application.Current.MainPage = new NavigationPage(new US_AccountsLogin(LoggedInUserId));
            }
            catch (Exception ex) { await DisplayAlert("Navigation Error", ex.Message, "OK"); }
        }

        private async void btnScrum_Tapped(object sender, TappedEventArgs e)
        {
            try
            {
                if (Navigation != null)
                    await Navigation.PushAsync(new Main_ScrumWindow(LoggedInUserId));
                else
                    Application.Current.MainPage = new NavigationPage(new Main_ScrumWindow(LoggedInUserId));
            }
            catch (Exception ex) { await DisplayAlert("Navigation Error", ex.Message, "OK"); }
        }

        private async void btnFileManager_Tapped(object sender, TappedEventArgs e)
        {
            try
            {
                if (Navigation != null)
                    await Navigation.PushAsync(new frmAdmin_DocInfo(LoggedInUserId));
                else
                    Application.Current.MainPage = new NavigationPage(new frmAdmin_DocInfo(LoggedInUserId));
            }
            catch (Exception ex) { await DisplayAlert("Navigation Error", ex.Message, "OK"); }
        }

        private void btnVideoConference_Tapped(object sender, TappedEventArgs e) { }
        private void btnRemoteAccess_Tapped(object sender, TappedEventArgs e) { }
    }
}