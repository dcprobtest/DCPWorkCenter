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
using ModelView_WPF.Helpers;

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

        private frmClass_ClassPlayer classPlayerWindow = null;
        private frmAdmin_DocInfo docManagerWindow = null;
        private Main_Adminfrm adminFormsWindow = null;
        private Main_Classfrm classFormsWindow = null;
        private Main_ScrumWindow scrumWindow = null;
        private RemoteAccessWindow remoteAccessWindow = null;

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

        protected override async void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (App.Current.Properties.Contains("CurrentSessionID"))
            {
                int sessionId = (int)App.Current.Properties["CurrentSessionID"];

                try
                {
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    };

                    using (HttpClient client = new HttpClient(handler))
                    {
                        var updateData = new
                        {
                            EndDate = DateTime.Now
                        };

                        await client.PutAsJsonAsync($"{ApiSettings.LoginSessionInfoUrl()}/{sessionId}", updateData);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error updating EndDate: " + ex.Message);
                }
            }
        }

        private void btnMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ChatBox chatWindow = new ChatBox(_loggedInUID);

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
            if (_userRole == 4)
            {
                MessageBox.Show("Your account status does not allow access to File Manager.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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
            if (_userRole == 2 || _userRole == 4)
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
                    classFormsWindow = new Main_Classfrm(_loggedInUID, _userRole);
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
            if (_userRole == 3 || _userRole == 4)
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
            if (_userRole == 3 || _userRole == 4)
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

        private void btnRemoteAccess_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1 && _userRole != 3 && _userRole != 4)
            {
                MessageBox.Show("You do not have permission to access Remote Access.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int userId = int.TryParse(_loggedInUID, out int parsedId) ? parsedId : 101;

                if (remoteAccessWindow == null || !IsWindowOpen(remoteAccessWindow))
                {
                    remoteAccessWindow = new RemoteAccessWindow(userId);
                    remoteAccessWindow.Closed += (s, args) =>
                    {
                        remoteAccessWindow = null;
                        this.Show();
                    };
                    this.Hide();
                    remoteAccessWindow.Show();
                }
                else
                {
                    if (remoteAccessWindow.WindowState == WindowState.Minimized)
                    {
                        remoteAccessWindow.WindowState = WindowState.Normal;
                    }
                    remoteAccessWindow.Show();
                    remoteAccessWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Remote Access: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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