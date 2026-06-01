using System;
using System.Windows;

// Using aliases to resolve potential naming conflicts
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace View_WPF
{
    public partial class MainWindow : Window
    {
        // Variables to store the logged-in user's ID, Name, and Role (Activate status)
        private string _loggedInUID = string.Empty;
        private string _loggedInName = string.Empty;
        private int _userRole;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Updated Constructor to receive UID, Name, and Activate status from the Login window
        public MainWindow(string uid, string name, int role)
        {
            InitializeComponent();
            _loggedInUID = uid;
            _loggedInName = name;
            _userRole = role;

            // Access control logic can be handled here if needed.
            // Setting the window title to display the logged-in user details.
            this.Title = $"Main Window - Welcome {name} (ID: {uid})";
        }

        // Click event for Video Class (Class Player) button
        private void btnVideoClass_Click(object sender, RoutedEventArgs e)
        {
            // Restrict access if the user status/role is 3
            if (_userRole == 3)
            {
                MessageBox.Show("Access Denied: Your account status does not allow access to Video Class.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                frmClass_ClassPlayer classPlayer = new frmClass_ClassPlayer(_loggedInUID);

                // Hide the main window
                this.Hide();

                // Subscribe to the Closed event to restore MainWindow
                classPlayer.Closed += (s, args) =>
                {
                    this.Show();
                };

                // Show the target window
                classPlayer.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Video Class: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Fallback to ensure MainWindow remains visible on failure
                this.Show();
            }
        }

        // Click event for File Manager button
        private void btnFileManager_Click(object sender, RoutedEventArgs e)
        {
            // Restrict access if the user status/role is 3
            if (_userRole == 3)
            {
                MessageBox.Show("Access Denied: Your account status does not allow access to File Manager.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                frmAdmin_DocInfo docManager = new frmAdmin_DocInfo(_loggedInUID);

                // Hide the main window
                this.Hide();

                // Subscribe to the Closed event to restore MainWindow
                docManager.Closed += (s, args) =>
                {
                    this.Show();
                };

                // Show the target window
                docManager.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening File Manager: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Fallback to ensure MainWindow remains visible on failure
                this.Show();
            }
        }

        // Click event for Admin Forms button
        private void btnAdminForms_Click(object sender, RoutedEventArgs e)
        {
            // Restrict access to Admin Forms for role status 2 and 3
            if (_userRole == 2 || _userRole == 3)
            {
                MessageBox.Show("Access Denied: You do not have permission to access Admin Forms.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Main_Adminfrm adminForms = new Main_Adminfrm(_loggedInUID, _loggedInName);

                // Hide the main window
                this.Hide();

                // Subscribe to the Closed event to restore MainWindow
                adminForms.Closed += (s, args) =>
                {
                    this.Show();
                };

                // Show the target window
                adminForms.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Admin Forms: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Fallback to ensure MainWindow remains visible on failure
                this.Show();
            }
        }

        // Click event for Class Forms button
        private void btnClassForms_Click(object sender, RoutedEventArgs e)
        {
            // Restrict access if the user status/role is 3
            if (_userRole == 3)
            {
                MessageBox.Show("Access Denied: Your account status does not allow access to Class Forms.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Main_Classfrm classForms = new Main_Classfrm(_loggedInUID);

                // Hide the main window
                this.Hide();

                // Subscribe to the Closed event to restore MainWindow
                classForms.Closed += (s, args) =>
                {
                    this.Show();
                };

                // Show the target window
                classForms.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Class Forms: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Fallback to ensure MainWindow remains visible on failure
                this.Show();
            }
        }

        // Click event for Accounts button
        private void btnAccounts_Click(object sender, RoutedEventArgs e)
        {
            // Accounts button is accessible to all authorized roles (1, 2, 3)
            try
            {
                // অ্যাপ্লিকেশনটিকে নিশ্চিত করা হলো যে এটিই আসল প্রধান উইন্ডো
                System.Windows.Application.Current.MainWindow = this;

                AccountsLogin accountsWindow = new AccountsLogin(_loggedInUID);

                // Hide the main window
                this.Hide();

                // Updated: Checking custom property IsLoginSuccess instead of DialogResult
                accountsWindow.Closed += (s, args) =>
                {
                    if (accountsWindow.IsLoginSuccess == false)
                    {
                        this.Show();
                    }
                };

                // Show the target window
                accountsWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Accounts Login: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Fallback to ensure MainWindow remains visible on failure
                this.Show();
            }
        }

        // Click event for US Accounts button
        private void btnUSAccounts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // অ্যাপ্লিকেশনটিকে নিশ্চিত করা হলো যে এটিই আসল প্রধান উইন্ডো
                System.Windows.Application.Current.MainWindow = this;

                // Opening the secure US Accounts Login window instead of the main form directly
                // Passing the logged-in user UID to initialize the security verification
                US_AccountsLogin usAccountsLoginWindow = new US_AccountsLogin(_loggedInUID);

                // Hide the main window
                this.Hide();

                // Updated: Checking custom property IsLoginSuccess instead of DialogResult
                usAccountsLoginWindow.Closed += (s, args) =>
                {
                    // যদি পাসওয়ার্ড না দিয়ে বা ভুল দিয়ে লগইন উইন্ডো কেটে দেয়, তবে MainWindow শো করবে
                    if (usAccountsLoginWindow.IsLoginSuccess == false)
                    {
                        this.Show();
                    }
                    else
                    {
                        // যদি লগইন সফল হয়, তবে একটু আগে ওপেন হওয়া Main_frmUSAccounts উইন্ডোটিকে ট্র্যাক করার লজিক
                        foreach (Window window in System.Windows.Application.Current.Windows)
                        {
                            if (window is Main_frmUSAccounts)
                            {
                                // Main_frmUSAccounts ফর্মটি যখন ক্লোজ করা হবে, তখন এই MainWindow আবার দৃশ্যমান হবে
                                window.Closed += (senderWin, eWin) =>
                                {
                                    this.Show();
                                };
                                break;
                            }
                        }
                    }
                };

                // Show the target window
                usAccountsLoginWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening US Accounts Login: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Fallback to ensure MainWindow remains visible on failure
                this.Show();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Logic for other buttons
        }
    }
}