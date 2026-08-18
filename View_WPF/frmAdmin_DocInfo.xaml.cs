using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using View_WPF.ModelView_WPF;

using TextBox = System.Windows.Controls.TextBox;
using Button = System.Windows.Controls.Button;
using System.Linq;

namespace View_WPF
{
    public partial class frmAdmin_DocInfo : Window
    {
        private UserDocInfoViewModel _viewModel;

        public frmAdmin_DocInfo(string uid)
        {
            InitializeComponent();

            _viewModel = new UserDocInfoViewModel();

            this.DataContext = _viewModel;

            this.Loaded += (s, e) =>
            {
                if (!string.IsNullOrEmpty(uid))
                {
                    _viewModel.UID = uid;
                    _viewModel.UserID = uid;

                    UpdateUIDInUI(uid);
                }
            };
        }

        private void UpdateUIDInUI(string uid)
        {
            var txtBox = this.FindName("txtUID") as TextBox;

            if (txtBox != null)
            {
                this.Dispatcher.Invoke(() =>
                {
                    txtBox.Text = uid;
                    txtBox.IsReadOnly = true;

                    BindingExpression binding = txtBox.GetBindingExpression(TextBox.TextProperty);
                    binding?.UpdateTarget();
                });
            }
        }

        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.DataContext = this.DataContext;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (this.WindowState == WindowState.Minimized)
            {
                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window is MainWindow mainWindow)
                    {
                        mainWindow.WindowState = WindowState.Normal;
                        mainWindow.Show();
                        mainWindow.Activate();
                        break;
                    }
                }
            }
        }
    }
}