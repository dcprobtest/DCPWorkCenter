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

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for Main_Accountsfrm.xaml
    /// </summary>
    public partial class Main_Accountsfrm : Window
    {
        // Variable to store the logged-in user's ID
        private string _userUID = string.Empty; // Initialized to clear CS8618 warning

        // Default constructor (if needed)
        public Main_Accountsfrm()
        {
            InitializeComponent();

            // এই উইন্ডোটি বন্ধ (Closed) হলে MainWindow শো করার জন্য ইভেন্ট সাবস্ক্রাইব করা হলো
            this.Closed += Main_Accountsfrm_Closed;
        }

        // Constructor to receive the UID sent from the login window
        public Main_Accountsfrm(string uid)
        {
            InitializeComponent();
            this._userUID = uid; // Saved the received ID

            // এই উইন্ডোটি বন্ধ (Closed) হলে MainWindow শো করার জন্য ইভেন্ট সাবস্ক্রাইব করা হলো
            this.Closed += Main_Accountsfrm_Closed;

            // Optional: Show ID in title or label
            // this.Title = "Accounts Dashboard - User ID: " + _userUID;
        }

        // Event handler that triggers when Main_Accountsfrm is closed
        private void Main_Accountsfrm_Closed(object sender, EventArgs e)
        {
            try
            {
                // FIX (CS0104): Ambigous reference কাটাতে explicit namespace (System.Windows) ব্যবহার করা হয়েছে
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
            // Opening frmAccEmpSalary and passing the current user UID
            frmAccEmpSalary salaryForm = new frmAccEmpSalary(_userUID);
            salaryForm.Show();
        }

        // Expense Bub button click logic
        private void btnExpBub_Click(object sender, RoutedEventArgs e)
        {
            frmAccExpBub expBubForm = new frmAccExpBub(_userUID);
            expBubForm.Show();
        }

        // Expend Head button click logic
        private void btnExpendHead_Click(object sender, RoutedEventArgs e)
        {
            frmAccExpendHead expendHeadForm = new frmAccExpendHead(_userUID);

            // FIX (CS0103): এখানে অবজেক্টের নাম সংশোধন করে expendHeadForm করা হয়েছে
            expendHeadForm.Show();
        }

        // Income Head button click logic
        private void btnIncomeHead_Click(object sender, RoutedEventArgs e)
        {
            frmAccIncomeHead incomeHeadForm = new frmAccIncomeHead(_userUID);
            incomeHeadForm.Show();
        }

        // Income Sub button click logic
        private void btnIncSub_Click(object sender, RoutedEventArgs e)
        {
            frmAccIncSub incSubForm = new frmAccIncSub(_userUID);
            incSubForm.Show();
        }
    }
}