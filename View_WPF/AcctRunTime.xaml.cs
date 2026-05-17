using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Input;

// Aliases to avoid ambiguity
using MessageBox = System.Windows.MessageBox;
using Color = System.Windows.Media.Color;
using Brushes = System.Windows.Media.Brushes;
using TextBox = System.Windows.Controls.TextBox;
using Button = System.Windows.Controls.Button;
using DatePicker = System.Windows.Controls.DatePicker;

namespace View_WPF
{
    public partial class AcctRunTime : Window
    {
        private Dictionary<string, FrameworkElement> controlRefs = new Dictionary<string, FrameworkElement>();

        public AcctRunTime()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            BtnOpenForm_Click(sender, e);
        }

        private void BtnOpenForm_Click(object sender, RoutedEventArgs e)
        {
            // Main Window setup
            Window dynamicWindow = new Window
            {
                Title = "User Registration",
                Width = 750,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                // BACKGROUND COLOR SET TO WHITE
                Background = Brushes.White,
                ResizeMode = ResizeMode.NoResize,
                AllowsTransparency = true,
                WindowStyle = WindowStyle.None
            };

            // 5 Second Rainbow Border for the Main Window
            Border windowBorder = new Border
            {
                BorderThickness = new Thickness(5),
                CornerRadius = new CornerRadius(15),
                BorderBrush = CreateRainbowBrush(5)
            };

            Grid mainContainer = new Grid { Margin = new Thickness(30) };

            for (int i = 0; i < 7; i++)
                mainContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            mainContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            controlRefs.Clear();

            TextBlock header = new TextBlock
            {
                Text = "USER REGISTRATION FORM",
                FontSize = 26,
                FontWeight = FontWeights.ExtraBold,
                Foreground = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 25)
            };
            Grid.SetRow(header, 0);
            Grid.SetColumnSpan(header, 2);
            mainContainer.Children.Add(header);

            // Adding Inputs
            AddStandardInput(mainContainer, "UID", "User ID (UID)", 1, 0);
            AddStandardInput(mainContainer, "EntryByUID", "Entry By UID", 1, 1);
            AddStandardInput(mainContainer, "Password", "Password", 2, 0);
            AddStandardInput(mainContainer, "Phone", "Phone Number", 2, 1);
            AddStandardInput(mainContainer, "Email", "Email Address", 3, 0);
            AddStandardInput(mainContainer, "Activate", "Status", 3, 1);
            AddStandardInput(mainContainer, "EntryDate", "Entry Date", 4, 0, true);
            AddStandardInput(mainContainer, "StartDate", "Start Date", 4, 1, true);
            AddStandardInput(mainContainer, "EndDate", "End Date", 5, 0, true);
            AddStandardInput(mainContainer, "LastUpdate", "Last Update", 5, 1, true);

            // Save Button
            Border btnBorder = new Border
            {
                Width = 320,
                Height = 60,
                Margin = new Thickness(0, 30, 0, 0),
                CornerRadius = new CornerRadius(30),
                BorderThickness = new Thickness(3),
                BorderBrush = CreateRainbowBrush(5)
            };

            Button btnSave = new Button
            {
                Content = "SAVE INFORMATION",
                Background = Brushes.White,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                BorderThickness = new Thickness(0),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            btnSave.Template = CreateRoundButtonTemplate();
            btnSave.PreviewMouseDown += (s, args) => ApplyBounceEffect(btnBorder);
            btnSave.Click += async (s, args) => await SendDataToApi();

            btnBorder.Child = btnSave;
            Grid.SetRow(btnBorder, 6);
            Grid.SetColumnSpan(btnBorder, 2);
            mainContainer.Children.Add(btnBorder);

            windowBorder.Child = mainContainer;
            dynamicWindow.Content = windowBorder;
            dynamicWindow.ShowDialog();
        }

        private void AddStandardInput(Grid grid, string key, string label, int row, int col, bool isDate = false)
        {
            Grid cell = new Grid { Margin = new Thickness(15, 8, 15, 8) };
            cell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock lbl = new TextBlock { Text = label, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(lbl, 0);
            cell.Children.Add(lbl);

            Border standardBorder = new Border
            {
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(8),
                BorderBrush = new SolidColorBrush(Color.FromRgb(44, 62, 80)),
                Background = Brushes.White,
                RenderTransformOrigin = new System.Windows.Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 1)
            };

            FrameworkElement input;
            if (isDate)
            {
                input = new DatePicker
                {
                    BorderThickness = new Thickness(0),
                    Height = 35,
                    SelectedDate = DateTime.Now,
                    VerticalContentAlignment = VerticalAlignment.Center
                };
            }
            else
            {
                // TextBox fixes: Added Margin and VerticalContentAlignment for perfect shape
                input = new TextBox
                {
                    BorderThickness = new Thickness(0),
                    Height = 35,
                    Margin = new Thickness(5, 0, 5, 0), // Added slight margin so it doesn't touch corners
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Padding = new Thickness(5, 0, 5, 0),
                    Background = Brushes.Transparent
                };
            }

            input.PreviewMouseDown += (s, e) => ApplyBounceEffect(standardBorder);

            standardBorder.Child = input;
            Grid.SetRow(standardBorder, 1);
            cell.Children.Add(standardBorder);

            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, col);
            grid.Children.Add(cell);
            controlRefs.Add(key, input);
        }

        private LinearGradientBrush CreateRainbowBrush(int seconds)
        {
            LinearGradientBrush brush = new LinearGradientBrush
            {
                StartPoint = new System.Windows.Point(0, 0),
                EndPoint = new System.Windows.Point(1, 1)
            };
            brush.GradientStops.Add(new GradientStop(Colors.Red, 0.0));
            brush.GradientStops.Add(new GradientStop(Colors.Orange, 0.2));
            brush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.4));
            brush.GradientStops.Add(new GradientStop(Colors.Green, 0.6));
            brush.GradientStops.Add(new GradientStop(Colors.Blue, 0.8));
            brush.GradientStops.Add(new GradientStop(Colors.Purple, 1.0));

            RotateTransform rt = new RotateTransform();
            brush.RelativeTransform = rt;
            DoubleAnimation animation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(seconds)) { RepeatBehavior = RepeatBehavior.Forever };
            rt.BeginAnimation(RotateTransform.AngleProperty, animation);
            return brush;
        }

        private void ApplyBounceEffect(FrameworkElement element)
        {
            ScaleTransform scale = element.RenderTransform as ScaleTransform ?? new ScaleTransform(1, 1);
            element.RenderTransform = scale;
            element.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
            DoubleAnimation bounce = new DoubleAnimation(1, 0.95, TimeSpan.FromMilliseconds(100)) { AutoReverse = true };
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, bounce);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, bounce);
        }

        private ControlTemplate CreateRoundButtonTemplate()
        {
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(25));
            border.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            border.AppendChild(content);
            template.VisualTree = border;
            return template;
        }

        private async Task SendDataToApi()
        {
            try
            {
                var userData = new
                {
                    uid = ((TextBox)controlRefs["UID"]).Text,
                    password = ((TextBox)controlRefs["Password"]).Text,
                    phone = ((TextBox)controlRefs["Phone"]).Text,
                    email = ((TextBox)controlRefs["Email"]).Text,
                    entryDate = ((DatePicker)controlRefs["EntryDate"]).SelectedDate,
                    startDate = ((DatePicker)controlRefs["StartDate"]).SelectedDate,
                    endDate = ((DatePicker)controlRefs["EndDate"]).SelectedDate,
                    lastUpdate = ((DatePicker)controlRefs["LastUpdate"]).SelectedDate,
                    entryByUid = ((TextBox)controlRefs["EntryByUID"]).Text,
                    activate = ((TextBox)controlRefs["Activate"]).Text
                };

                using (HttpClient client = new HttpClient())
                {
                    var json = JsonSerializer.Serialize(userData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    await client.PostAsync("https://localhost:7073/api/Data/SaveUser", content);
                    MessageBox.Show("Done!", "Success");
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}