using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

// Using aliases to resolve potential naming conflicts
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for US_AccountsLogin.xaml
    /// </summary>
    public partial class US_AccountsLogin : Window
    {
        // Variable to store the logged-in user's UID (Initialized to avoid CS8618)
        private string _loggedInUID = string.Empty;

        // Custom property to track login success safely without using DialogResult
        public bool IsLoginSuccess { get; set; } = false;

        // Default constructor
        public US_AccountsLogin()
        {
            InitializeComponent();
        }

        // Parameterized constructor to receive UID from MainWindow
        public US_AccountsLogin(string uid)
        {
            InitializeComponent();
            _loggedInUID = uid;

            // Binding the UID to the TextBox (txtAccountUserID) via DataContext or direct assignment
            // Since txtAccountUserID has Text="{Binding AdminID}", we can set the DataContext
            this.DataContext = new { AdminID = _loggedInUID };
        }

        // Click event handler for the SECURE ACCESS button
        private void btnAccountLogin_Click(object sender, RoutedEventArgs e)
        {
            // Simple validation for PasswordBox
            if (string.IsNullOrEmpty(pbAccountPassword.Password))
            {
                MessageBox.Show("Please enter your password to access the administrative portal.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                pbAccountPassword.Focus();
                return;
            }

            try
            {
                // TODO: You can add your database or specific password validation logic here if needed
                // e.g., if (pbAccountPassword.Password == "YourSecurePassword") { ... }

                Main_frmUSAccounts usAccountsWindow = null;

                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window is Main_frmUSAccounts existingForm)
                    {
                        usAccountsWindow = existingForm;
                        break;
                    }
                }

                if (usAccountsWindow != null)
                {
                    if (usAccountsWindow.WindowState == WindowState.Minimized)
                    {
                        usAccountsWindow.WindowState = WindowState.Normal;
                    }
                    usAccountsWindow.Show();
                    usAccountsWindow.Activate();
                }
                else
                {
                    usAccountsWindow = new Main_frmUSAccounts(_loggedInUID);
                    usAccountsWindow.Show();
                }

                // Set custom property to true instead of DialogResult to bypass WPF Exception
                this.IsLoginSuccess = true;

                // Closing the current security login window after successful navigation
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening US Accounts Management: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}