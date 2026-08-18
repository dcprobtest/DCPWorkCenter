using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        private frmClass_ClassPlayer classPlayerWindow = null;
        private frmAdmin_DocInfo docManagerWindow = null;
        private Main_Adminfrm adminFormsWindow = null;
        private Main_Classfrm classFormsWindow = null;
        private Main_ScrumWindow scrumWindow = null;

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
            try
            {
                if (classPlayerWindow == null || !IsWindowOpen(classPlayerWindow))
                {
                    classPlayerWindow = new frmClass_ClassPlayer(_loggedInUID);
                    classPlayerWindow.Closed += (s, args) =>
                    {
                        classPlayerWindow = null;
                        this.Show();
                    };
                    this.Hide();
                    classPlayerWindow.Show();
                }
                else
                {
                    if (classPlayerWindow.WindowState == WindowState.Minimized)
                    {
                        classPlayerWindow.WindowState = WindowState.Normal;
                    }
                    classPlayerWindow.Show();
                    classPlayerWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Video Class: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Show();
            }
        }

        private void btnFileManager_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (docManagerWindow == null || !IsWindowOpen(docManagerWindow))
                {
                    docManagerWindow = new frmAdmin_DocInfo(_loggedInUID);
                    docManagerWindow.Closed += (s, args) =>
                    {
                        docManagerWindow = null;
                        this.Show();
                    };
                    this.Hide();
                    docManagerWindow.Show();
                }
                else
                {
                    if (docManagerWindow.WindowState == WindowState.Minimized)
                    {
                        docManagerWindow.WindowState = WindowState.Normal;
                    }
                    docManagerWindow.Show();
                    docManagerWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening File Manager: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Show();
            }
        }

        private void btnAdminForms_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole == 2)
            {
                MessageBox.Show("You do not have permission to access Admin Forms.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (adminFormsWindow == null || !IsWindowOpen(adminFormsWindow))
                {
                    adminFormsWindow = new Main_Adminfrm(_loggedInUID, _loggedInName, _userRole);
                    adminFormsWindow.Closed += (s, args) =>
                    {
                        adminFormsWindow = null;
                        this.Show();
                    };
                    this.Hide();
                    adminFormsWindow.Show();
                }
                else
                {
                    if (adminFormsWindow.WindowState == WindowState.Minimized)
                    {
                        adminFormsWindow.WindowState = WindowState.Normal;
                    }
                    adminFormsWindow.Show();
                    adminFormsWindow.Activate();
                }
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
                MessageBox.Show("Your account status does not allow access to Class Forms.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (classFormsWindow == null || !IsWindowOpen(classFormsWindow))
                {
                    classFormsWindow = new Main_Classfrm(_loggedInUID);
                    classFormsWindow.Closed += (s, args) =>
                    {
                        classFormsWindow = null;
                        this.Show();
                    };
                    this.Hide();
                    classFormsWindow.Show();
                }
                else
                {
                    if (classFormsWindow.WindowState == WindowState.Minimized)
                    {
                        classFormsWindow.WindowState = WindowState.Normal;
                    }
                    classFormsWindow.Show();
                    classFormsWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Class Forms: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Show();
            }
        }

        private void btnAccounts_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole == 3)
            {
                MessageBox.Show("Your account status does not allow access to Accounts.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            if (_userRole == 3)
            {
                MessageBox.Show("Your account status does not allow access to US Accounts.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            if (_userRole == 3)
            {
                MessageBox.Show("Your account status does not allow access to Scrum.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_userRole != 1)
            {
                MessageBox.Show("Your account status does not allow access to Scrum.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (scrumWindow == null || !IsWindowOpen(scrumWindow))
                {
                    scrumWindow = new Main_ScrumWindow(_loggedInUID);
                    scrumWindow.Closed += (s, args) =>
                    {
                        scrumWindow = null;
                        this.Show();
                    };
                    this.Hide();
                    scrumWindow.Show();
                }
                else
                {
                    if (scrumWindow.WindowState == WindowState.Minimized)
                    {
                        scrumWindow.WindowState = WindowState.Normal;
                    }
                    scrumWindow.Show();
                    scrumWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Scrum Window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Show();
            }
        }

        private bool IsWindowOpen(Window window)
        {
            return System.Windows.Application.Current.Windows.Cast<Window>().Any(w => w == window);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}