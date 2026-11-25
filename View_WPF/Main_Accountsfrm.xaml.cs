using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for Main_Accountsfrm.xaml
    /// </summary>
    public partial class Main_Accountsfrm : Window
    {
        // Variable to store the logged-in user's ID
        private string _userUID = string.Empty; // Initialized to clear CS8618 warning

        private frmAccEmpSalary salaryForm = null;
        private Login accAdminLoginForm = null;

        // Default constructor (if needed)
        public Main_Accountsfrm()
        {
            InitializeComponent();

            // Subscribe to the Closed event to show MainWindow when this window is closed
            this.Closed += Main_Accountsfrm_Closed;
        }

        // Constructor to receive the UID sent from the login window
        public Main_Accountsfrm(string uid)
        {
            InitializeComponent();
            this._userUID = uid; // Saved the received ID

            // Subscribe to the Closed event to show MainWindow when this window is closed
            this.Closed += Main_Accountsfrm_Closed;

            // Optional: Show ID in title or label
            // this.Title = "Accounts Dashboard - User ID: " + _userUID;
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

        // Event handler that triggers when Main_Accountsfrm is closed
        private void Main_Accountsfrm_Closed(object sender, EventArgs e)
        {
            try
            {
                // FIX (CS0104): Used explicit namespace (System.Windows) to resolve ambiguous reference
                if (System.Windows.Application.Current.MainWindow != null)
                {
                    System.Windows.Application.Current.MainWindow.Show();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error returning to Main Window: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Employee Salary button click logic
        private void btnEmpSalary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (salaryForm == null || !IsWindowOpen(salaryForm))
                {
                    salaryForm = new frmAccEmpSalary(_userUID);
                    salaryForm.Closed += (s, args) => { salaryForm = null; };
                    salaryForm.Show();
                }
                else
                {
                    if (salaryForm.WindowState == WindowState.Minimized)
                    {
                        salaryForm.WindowState = WindowState.Normal;
                    }
                    salaryForm.Show();
                    salaryForm.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Employee Salary: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Accounts Admin button click logic -> Opens existing Login window in Accounts Admin mode
        private void btnAccAdmin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (accAdminLoginForm == null || !IsWindowOpen(accAdminLoginForm))
                {
                    accAdminLoginForm = new Login(true);
                    accAdminLoginForm.Closed += (s, args) => { accAdminLoginForm = null; };
                    accAdminLoginForm.Show();
                }
                else
                {
                    if (accAdminLoginForm.WindowState == WindowState.Minimized)
                    {
                        accAdminLoginForm.WindowState = WindowState.Normal;
                    }
                    accAdminLoginForm.Show();
                    accAdminLoginForm.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Accounts Admin Login: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}