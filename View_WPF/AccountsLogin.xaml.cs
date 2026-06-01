using System;
using System.Windows;
using System.Windows.Controls;
// Add your database or API references here if needed
// using YourProject.Services; 

using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for AccountsLogin.xaml
    /// </summary>
    public partial class AccountsLogin : Window
    {
        // 1. Create the property that the TextBox is bound to
        public string AdminID { get; set; }

        // Variable to store the UID passed from MainWindow
        private string _loggedInUID;

        // Custom property to track login success safely without using DialogResult
        public bool IsLoginSuccess { get; set; } = false;

        public AccountsLogin()
        {
            InitializeComponent();
        }

        // Constructor to receive UID from MainWindow
        public AccountsLogin(string uid)
        {
            InitializeComponent();

            // 2. Set the received ID to the property and variable
            _loggedInUID = uid;
            AdminID = uid;

            // 3. Important: Set the DataContext of the window so XAML binding works
            this.DataContext = this;

            // 4. Set the value directly in the TextBox to ensure it is shown (for safety)
            txtAccountUserID.Text = AdminID;

            // Since the ID is provided automatically, set it to Read-Only so the user cannot change it
            txtAccountUserID.IsReadOnly = true;
        }

        private void btnAccountLogin_Click(object sender, RoutedEventArgs e)
        {
            string adminId = txtAccountUserID.Text.Trim();
            string password = pbAccountPassword.Password.Trim();

            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter your password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                /* Step: Database Verification (tblUser)
                   You need to check the password from your server or database here.
                */

                bool isAuthenticated = CheckUserPasswordFromDatabase(adminId, password);

                if (isAuthenticated)
                {
                    // Open Main_Accountsfrm if the password is correct
                    Main_Accountsfrm accountsForm = new Main_Accountsfrm(_loggedInUID);
                    accountsForm.Show();

                    // Set custom property to true instead of DialogResult to bypass WPF Exception
                    this.IsLoginSuccess = true;

                    // Close the current login window
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Incorrect password! Please try again.", "Access Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // A dummy method to check password from the database
        private bool CheckUserPasswordFromDatabase(string uid, string pass)
        {
            // Check the password against this UID in your tblUser table
            // Set to true for testing purposes for now
            return true;
        }
    }
}