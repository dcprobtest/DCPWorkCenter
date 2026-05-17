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
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for Main_frmUSAccounts.xaml
    /// </summary>
    public partial class Main_frmUSAccounts : Window
    {
        // Variable to store the logged-in user's UID
        private string _loggedInUID;

        // Default constructor (for general usage if needed)
        public Main_frmUSAccounts()
        {
            InitializeComponent();
        }

        // Parameterized constructor to receive UID from MainWindow
        public Main_frmUSAccounts(string uid)
        {
            InitializeComponent();
            _loggedInUID = uid; // Setting the passed UID to the private variable

            // Setting the window title to display the logged-in user ID
            this.Title = $"US Accounts - Logged in as: {uid}";
        }

        // Click event for Acc US Exp Bub button
        private void btnAccUSExpBub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Opening frmAccUSExpBub form and passing the logged-in UID
                frmAccUSExpBub expBubWindow = new frmAccUSExpBub(_loggedInUID);
                expBubWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Acc US Exp Bub: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}