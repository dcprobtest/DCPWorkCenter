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
        private string _userUID;

        // Default constructor (if needed)
        public Main_Accountsfrm()
        {
            InitializeComponent();
        }

        // Constructor to receive the UID sent from the login window
        public Main_Accountsfrm(string uid)
        {
            InitializeComponent();
            this._userUID = uid; // Saved the received ID

            // Optional: Show ID in title or label
            // this.Title = "Accounts Dashboard - User ID: " + _userUID;
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