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
        private string _loggedInUID;
        private string _loggedInName;
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

            // যেহেতু বাটন লুকানোর দরকার নেই, তাই ApplyAccessControl কল করার প্রয়োজন নেই।
            // তবে আপনি চাইলে টাইটেল বারে ইউজারের নাম সেট করতে পারেন।
            this.Title = $"Main Window - Welcome {name} (ID: {uid})";
        }

        // Click event for Video Class (Class Player) button
        private void btnVideoClass_Click(object sender, RoutedEventArgs e)
        {
            // Activate 3 হলে এই সেকশনে ঢোকা নিষেধ
            if (_userRole == 3)
            {
                MessageBox.Show("Access Denied: Your account status does not allow access to Video Class.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                frmClass_ClassPlayer classPlayer = new frmClass_ClassPlayer(_loggedInUID);
                classPlayer.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Video Class: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Click event for File Manager button
        private void btnFileManager_Click(object sender, RoutedEventArgs e)
        {
            // Activate 3 হলে এই সেকশনে ঢোকা নিষেধ
            if (_userRole == 3)
            {
                MessageBox.Show("Access Denied: Your account status does not allow access to File Manager.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                frmAdmin_DocInfo docManager = new frmAdmin_DocInfo(_loggedInUID);
                docManager.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening File Manager: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Click event for Admin Forms button
        private void btnAdminForms_Click(object sender, RoutedEventArgs e)
        {
            // Activate 2 বা 3 কারোরই Admin Forms-এ ঢোকা উচিত নয় (লজিক অনুযায়ী)
            if (_userRole == 2 || _userRole == 3)
            {
                MessageBox.Show("Access Denied: You do not have permission to access Admin Forms.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Main_Adminfrm adminForms = new Main_Adminfrm(_loggedInUID, _loggedInName);
                adminForms.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Admin Forms: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Click event for Class Forms button
        private void btnClassForms_Click(object sender, RoutedEventArgs e)
        {
            // Activate 3 হলে এই সেকশনে ঢোকা নিষেধ
            if (_userRole == 3)
            {
                MessageBox.Show("Access Denied: Your account status does not allow access to Class Forms.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Main_Classfrm classForms = new Main_Classfrm(_loggedInUID);
                classForms.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Class Forms: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Click event for Accounts button
        private void btnAccounts_Click(object sender, RoutedEventArgs e)
        {
            // Accounts বাটন সবার জন্য খোলা (Role 1, 2, 3 সবাই ঢুকতে পারবে)
            try
            {
                AccountsLogin accountsWindow = new AccountsLogin(_loggedInUID);
                accountsWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Accounts Login: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Click event for US Accounts button
        private void btnUSAccounts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Main_frmUSAccounts ফর্ম ওপেন করা হচ্ছে এবং সেখানে UID পাস করা হচ্ছে
                Main_frmUSAccounts usAccountsWindow = new Main_frmUSAccounts(_loggedInUID);
                usAccountsWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening US Accounts: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Logic for other buttons
        }
    }
}