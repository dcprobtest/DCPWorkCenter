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

using MessageBox = System.Windows.MessageBox;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for Main_Adminfrm.xaml
    /// </summary>
    public partial class Main_Adminfrm : Window
    {
        private string _uid;
        private string _userName;
        private int _userRole;

        private frmAdmin_Department deptWin = null;
        private frmAdmin_Designation desigWin = null;
        private frm_STEM stemWin = null;
        private frmAdmin_EmpInfo empWin = null;
        private frmAdmin_User userWin = null;
        private frm_createdocx docxWin = null;
        private frm_ClassFocusMonitor classFocusMonitorWin = null;
        private frm_ScreenShotViewer screenshotWin = null;
        private ChatBox chatWin = null;

        public Main_Adminfrm()
        {
            InitializeComponent();
        }

        public Main_Adminfrm(string uid, string name)
        {
            InitializeComponent();
            _uid = uid;
            _userName = name;
        }

        public Main_Adminfrm(string uid, string name, int role)
        {
            InitializeComponent();
            _uid = uid;
            _userName = name;
            _userRole = role;
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

        private void BtnDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1)
            {
                MessageBox.Show("Your account status does not allow access to Department.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (deptWin == null || !IsWindowOpen(deptWin))
                {
                    deptWin = new frmAdmin_Department(_uid);
                    deptWin.Closed += (s, args) => { deptWin = null; };
                    deptWin.Show();
                }
                else
                {
                    if (deptWin.WindowState == WindowState.Minimized)
                    {
                        deptWin.WindowState = WindowState.Normal;
                    }
                    deptWin.Show();
                    deptWin.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Department Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDesignation_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1)
            {
                MessageBox.Show("Your account status does not allow access to Designation.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (desigWin == null || !IsWindowOpen(desigWin))
                {
                    desigWin = new frmAdmin_Designation(_uid);
                    desigWin.Closed += (s, args) => { desigWin = null; };
                    desigWin.Show();
                }
                else
                {
                    if (desigWin.WindowState == WindowState.Minimized)
                    {
                        desigWin.WindowState = WindowState.Normal;
                    }
                    desigWin.Show();
                    desigWin.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Designation Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSTEM_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1)
            {
                MessageBox.Show("Your account status does not allow access to STEM.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (stemWin == null || !IsWindowOpen(stemWin))
                {
                    stemWin = new frm_STEM(_uid);
                    stemWin.Closed += (s, args) => { stemWin = null; };
                    stemWin.Show();
                }
                else
                {
                    if (stemWin.WindowState == WindowState.Minimized)
                    {
                        stemWin.WindowState = WindowState.Normal;
                    }
                    stemWin.Show();
                    stemWin.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening STEM Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnScreenshotViewer_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1)
            {
                MessageBox.Show("Your account status does not allow access to Screenshot Viewer.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (screenshotWin == null || !IsWindowOpen(screenshotWin))
                {
                    screenshotWin = new frm_ScreenShotViewer();
                    screenshotWin.Closed += (s, args) => { screenshotWin = null; };
                    screenshotWin.Show();
                }
                else
                {
                    if (screenshotWin.WindowState == WindowState.Minimized)
                    {
                        screenshotWin.WindowState = WindowState.Normal;
                    }
                    screenshotWin.Show();
                    screenshotWin.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Screenshot Viewer: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEmployeeInfo_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1 && _userRole != 3)
            {
                MessageBox.Show("Your account status does not allow access to Employee Info.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (empWin == null || !IsWindowOpen(empWin))
                {
                    empWin = new frmAdmin_EmpInfo(_uid);
                    empWin.Closed += (s, args) => { empWin = null; };
                    empWin.Show();
                }
                else
                {
                    if (empWin.WindowState == WindowState.Minimized)
                    {
                        empWin.WindowState = WindowState.Normal;
                    }
                    empWin.Show();
                    empWin.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Employee Info: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnUserTable_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1)
            {
                MessageBox.Show("Your account status does not allow access to User Table.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (userWin == null || !IsWindowOpen(userWin))
                {
                    userWin = new frmAdmin_User();
                    userWin.Closed += (s, args) => { userWin = null; };
                    userWin.Show();
                }
                else
                {
                    if (userWin.WindowState == WindowState.Minimized)
                    {
                        userWin.WindowState = WindowState.Normal;
                    }
                    userWin.Show();
                    userWin.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening User Table: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCreateDocx_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1)
            {
                MessageBox.Show("Your account status does not allow access to Create Docx.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (docxWin == null || !IsWindowOpen(docxWin))
                {
                    docxWin = new frm_createdocx(_uid, _userName);
                    docxWin.Closed += (s, args) => { docxWin = null; };
                    docxWin.Show();
                }
                else
                {
                    if (docxWin.WindowState == WindowState.Minimized)
                    {
                        docxWin.WindowState = WindowState.Normal;
                    }
                    docxWin.Show();
                    docxWin.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Document Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClassFocusMonitor_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole != 1)
            {
                MessageBox.Show("Your account status does not allow access to Class Focus Monitor.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (classFocusMonitorWin == null || !IsWindowOpen(classFocusMonitorWin))
                {
                    classFocusMonitorWin = new frm_ClassFocusMonitor();
                    classFocusMonitorWin.Closed += (s, args) => { classFocusMonitorWin = null; };
                    classFocusMonitorWin.Show();
                }
                else
                {
                    if (classFocusMonitorWin.WindowState == WindowState.Minimized)
                    {
                        classFocusMonitorWin.WindowState = WindowState.Normal;
                    }
                    classFocusMonitorWin.Show();
                    classFocusMonitorWin.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Class Focus Monitor: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (chatWin == null || !IsWindowOpen(chatWin))
                {
                    chatWin = new ChatBox(_uid);
                    chatWin.Closed += (s, args) => { chatWin = null; };
                    chatWin.Show();
                }
                else
                {
                    if (chatWin.WindowState == WindowState.Minimized)
                    {
                        chatWin.WindowState = WindowState.Normal;
                    }
                    chatWin.Show();
                    chatWin.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Message Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}