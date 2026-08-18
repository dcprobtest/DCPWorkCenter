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
        private string _loggedInUID = string.Empty;

        private frmAccUSExpBub expBubWindow = null;
        private frmAccUSExpendHead expendHeadWindow = null;
        private frmAccUSIncomeHead incomeHeadWindow = null;
        private frmAccUSIncSub incomeSubWindow = null;

        // Default constructor (for general usage if needed)
        public Main_frmUSAccounts()
        {
            InitializeComponent();

            // Subscribe to the Closed event to show MainWindow when this window is closed
            this.Closed += Main_frmUSAccounts_Closed;
        }

        // Parameterized constructor to receive UID from US_AccountsLogin window
        public Main_frmUSAccounts(string uid)
        {
            InitializeComponent();
            _loggedInUID = uid; // Setting the passed UID to the private variable

            // Subscribe to the Closed event to show MainWindow when this window is closed
            this.Closed += Main_frmUSAccounts_Closed;

            // Setting the window title to display the logged-in user ID
            this.Title = $"US Accounts - Logged in as: {uid}";
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

        private bool IsWindowOpen(Window window)
        {
            return System.Windows.Application.Current.Windows.Cast<Window>().Any(w => w == window);
        }

        // Event handler that triggers when Main_frmUSAccounts is closed
        private void Main_frmUSAccounts_Closed(object sender, EventArgs e)
        {
            try
            {
                // Used explicit namespace (System.Windows) to resolve ambiguous reference and show MainWindow
                if (System.Windows.Application.Current.MainWindow != null)
                {
                    System.Windows.Application.Current.MainWindow.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error returning to Main Window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Click event for Acc US Exp Bub button
        private void btnAccUSExpBub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (expBubWindow == null || !IsWindowOpen(expBubWindow))
                {
                    expBubWindow = new frmAccUSExpBub(_loggedInUID);
                    expBubWindow.Closed += (s, args) => { expBubWindow = null; };
                    expBubWindow.Show();
                }
                else
                {
                    if (expBubWindow.WindowState == WindowState.Minimized)
                    {
                        expBubWindow.WindowState = WindowState.Normal;
                    }
                    expBubWindow.Show();
                    expBubWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Acc US Exp Bub: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Click event for Acc US Expend Head button
        private void btnAccUSExpendHead_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (expendHeadWindow == null || !IsWindowOpen(expendHeadWindow))
                {
                    expendHeadWindow = new frmAccUSExpendHead(_loggedInUID);
                    expendHeadWindow.Closed += (s, args) => { expendHeadWindow = null; };
                    expendHeadWindow.Show();
                }
                else
                {
                    if (expendHeadWindow.WindowState == WindowState.Minimized)
                    {
                        expendHeadWindow.WindowState = WindowState.Normal;
                    }
                    expendHeadWindow.Show();
                    expendHeadWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Acc US Expend Head: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Click event for Acc US Income Head button
        private void btnAccUSIncomeHead_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (incomeHeadWindow == null || !IsWindowOpen(incomeHeadWindow))
                {
                    incomeHeadWindow = new frmAccUSIncomeHead(_loggedInUID);
                    incomeHeadWindow.Closed += (s, args) => { incomeHeadWindow = null; };
                    incomeHeadWindow.Show();
                }
                else
                {
                    if (incomeHeadWindow.WindowState == WindowState.Minimized)
                    {
                        incomeHeadWindow.WindowState = WindowState.Normal;
                    }
                    incomeHeadWindow.Show();
                    incomeHeadWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Acc US Income Head: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Click event for Acc US Inc Sub button
        private void btnAccUSIncSub_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (incomeSubWindow == null || !IsWindowOpen(incomeSubWindow))
                {
                    incomeSubWindow = new frmAccUSIncSub(_loggedInUID);
                    incomeSubWindow.Closed += (s, args) => { incomeSubWindow = null; };
                    incomeSubWindow.Show();
                }
                else
                {
                    if (incomeSubWindow.WindowState == WindowState.Minimized)
                    {
                        incomeSubWindow.WindowState = WindowState.Normal;
                    }
                    incomeSubWindow.Show();
                    incomeSubWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Acc US Inc Sub: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}