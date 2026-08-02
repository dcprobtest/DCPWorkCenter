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

// Alias added to resolve potential naming conflicts
using MessageBox = System.Windows.MessageBox;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for Main_Adminfrm.xaml
    /// </summary>
    public partial class Main_Adminfrm : Window
    {
        // Private variables to hold logged-in user ID and name
        private string _uid;
        private string _userName;
        private int _userRole;

        // Default constructor
        public Main_Adminfrm()
        {
            InitializeComponent();
        }

        // Constructor to receive UID and Name from MainWindow
        public Main_Adminfrm(string uid, string name)
        {
            InitializeComponent();
            _uid = uid;
            _userName = name;
        }

        // Overloaded constructor to receive role as well
        public Main_Adminfrm(string uid, string name, int role)
        {
            InitializeComponent();
            _uid = uid;
            _userName = name;
            _userRole = role;
        }

        // Department button click event
        private void BtnDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1)
            {
                MessageBox.Show("Your account status does not allow access to Department.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                frmAdmin_Department deptWin = new frmAdmin_Department();
                deptWin.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Department Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Designation button click event
        private void BtnDesignation_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1)
            {
                MessageBox.Show("Your account status does not allow access to Designation.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                frmAdmin_Designation desigWin = new frmAdmin_Designation();
                desigWin.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Designation Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // STEM button click event
        private void BtnSTEM_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1)
            {
                MessageBox.Show("Your account status does not allow access to STEM.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                frm_STEM stemWin = new frm_STEM(_uid);
                stemWin.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening STEM Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Employee Info button click event
        private void BtnEmployeeInfo_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1 && _userRole != 3)
            {
                MessageBox.Show("Your account status does not allow access to Employee Info.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                frmAdmin_EmpInfo empWin = new frmAdmin_EmpInfo(_uid);
                empWin.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Employee Info: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // User Table button click event
        private void BtnUserTable_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1)
            {
                MessageBox.Show("Your account status does not allow access to User Table.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                frmAdmin_User userWin = new frmAdmin_User();
                userWin.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening User Table: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Create Docx button click event
        private void BtnCreateDocx_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1)
            {
                MessageBox.Show("Your account status does not allow access to Create Docx.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                frm_createdocx docxWin = new frm_createdocx(_uid, _userName);
                docxWin.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Document Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Message button click event to open chatBox
        private void BtnMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Opening chatBox and passing the current _uid
                ChatBox chatWin = new ChatBox(_uid);
                chatWin.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Message Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}