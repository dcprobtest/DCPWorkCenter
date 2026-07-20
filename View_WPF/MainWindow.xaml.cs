using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Net.Http;
using System.Net.Http.Json;
using System.ComponentModel;

// Using aliases to resolve potential naming conflicts
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace View_WPF
{
    public partial class MainWindow : Window
    {
        private string _loggedInUID = string.Empty;
        private string _loggedInName = string.Empty;
        private int _userRole;

        // Base URL for API
        private readonly string apiBaseUrl = "https://localhost:7073/";

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(string uid, string name, int role)
        {
            InitializeComponent();
            _loggedInUID = uid;
            _loggedInName = name;
            _userRole = role;
            this.Title = $"Main Window - Welcome {name} (ID: {uid})";
        }

        // Added logic to handle EndDate update on Window close
        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (App.Current.Properties.Contains("CurrentSessionID"))
            {
                int sessionId = (int)App.Current.Properties["CurrentSessionID"];

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var updateData = new
                        {
                            EndDate = DateTime.Now
                        };

                        await client.PutAsJsonAsync($"{apiBaseUrl}api/LoginSessionInfo/{sessionId}", updateData);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error updating EndDate: " + ex.Message);
                }
            }
        }

        // Updated Message button click event
        private void btnMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open ChatBox
                ChatBox chatWindow = new ChatBox(_loggedInUID);

                // Set position of the ChatBox
                chatWindow.WindowStartupLocation = WindowStartupLocation.Manual;
                chatWindow.Left = this.Left + this.Width - 400;
                chatWindow.Top = this.Top + 50;

                chatWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening ChatBox: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnVideoClass_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole == 3)
            {
                MessageBox.Show("Access Denied: Your account status does not allow access to Video Class.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                frmClass_ClassPlayer classPlayer = new frmClass_ClassPlayer(_loggedInUID);
                this.Hide();
                classPlayer.Closed += (s, args) => { this.Show(); };
                classPlayer.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Video Class: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Show();
            }
        }

        private void btnFileManager_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole == 3)
            {
                MessageBox.Show("Access Denied: Your account status does not allow access to File Manager.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                frmAdmin_DocInfo docManager = new frmAdmin_DocInfo(_loggedInUID);
                this.Hide();
                docManager.Closed += (s, args) => { this.Show(); };
                docManager.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening File Manager: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Show();
            }
        }

        private void btnAdminForms_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole == 2 || _userRole == 3)
            {
                MessageBox.Show("Access Denied: You do not have permission to access Admin Forms.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Main_Adminfrm adminForms = new Main_Adminfrm(_loggedInUID, _loggedInName);
                this.Hide();
                adminForms.Closed += (s, args) => { this.Show(); };
                adminForms.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Admin Forms: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Show();
            }
        }

        private void btnClassForms_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole == 3)
            {
                MessageBox.Show("Access Denied: Your account status does not allow access to Class Forms.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Main_Classfrm classForms = new Main_Classfrm(_loggedInUID);
                this.Hide();
                classForms.Closed += (s, args) => { this.Show(); };
                classForms.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Class Forms: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Show();
            }
        }

        private void btnAccounts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Application.Current.MainWindow = this;
                AccountsLogin accountsWindow = new AccountsLogin(_loggedInUID);
                this.Hide();
                accountsWindow.Closed += (s, args) => { if (accountsWindow.IsLoginSuccess == false) { this.Show(); } };
                accountsWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Accounts Login: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Show();
            }
        }

        private void btnUSAccounts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Application.Current.MainWindow = this;
                US_AccountsLogin usAccountsLoginWindow = new US_AccountsLogin(_loggedInUID);
                this.Hide();
                usAccountsLoginWindow.Closed += (s, args) =>
                {
                    if (usAccountsLoginWindow.IsLoginSuccess == false)
                    {
                        this.Show();
                    }
                    else
                    {
                        foreach (Window window in System.Windows.Application.Current.Windows)
                        {
                            if (window is Main_frmUSAccounts)
                            {
                                window.Closed += (senderWin, eWin) => { this.Show(); };
                                break;
                            }
                        }
                    }
                };
                usAccountsLoginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening US Accounts Login: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Show();
            }
        }

        private void btnScrum_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Main_ScrumWindow scrumWindow = new Main_ScrumWindow(_loggedInUID);
                this.Hide();
                scrumWindow.Closed += (s, args) => { this.Show(); };
                scrumWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Scrum Window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Show();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}