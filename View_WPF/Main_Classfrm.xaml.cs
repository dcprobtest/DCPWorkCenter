using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace View_WPF
{
    public partial class Main_Classfrm : Window
    {
        private string _uID;
        private int _userRole;

        private frmClass_ClassSubject courseSubjectWindow = null;
        private frm_ClassSection subjectSectionWindow = null;
        private frmClass_SubjectWiseClassAssign sectionAssignWindow = null;
        private frmClass_ClassVideoText videoUploadWindow = null;
        private ChatBox chatWin = null;

        public Main_Classfrm()
        {
            InitializeComponent();
            _userRole = 1;
        }

        public Main_Classfrm(string uid, int role = 1)
        {
            InitializeComponent();
            _uID = uid;
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

        private void btnClassForms_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("You are currently in the Course & Content Management form.");
        }

        private void btnCourseSubject_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole == 4)
            {
                System.Windows.MessageBox.Show("Your account status does not allow access to Course Subject.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (courseSubjectWindow == null || !IsWindowOpen(courseSubjectWindow))
                {
                    courseSubjectWindow = new frmClass_ClassSubject(_uID);
                    courseSubjectWindow.Closed += (s, args) => { courseSubjectWindow = null; };
                    courseSubjectWindow.Show();
                }
                else
                {
                    if (courseSubjectWindow.WindowState == WindowState.Minimized)
                    {
                        courseSubjectWindow.WindowState = WindowState.Normal;
                    }
                    courseSubjectWindow.Show();
                    courseSubjectWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error opening Course Subject Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSubjectSection_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole == 4)
            {
                System.Windows.MessageBox.Show("Your account status does not allow access to Subject Section.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (subjectSectionWindow == null || !IsWindowOpen(subjectSectionWindow))
                {
                    subjectSectionWindow = new frm_ClassSection(_uID);
                    subjectSectionWindow.Closed += (s, args) => { subjectSectionWindow = null; };
                    subjectSectionWindow.Show();
                }
                else
                {
                    if (subjectSectionWindow.WindowState == WindowState.Minimized)
                    {
                        subjectSectionWindow.WindowState = WindowState.Normal;
                    }
                    subjectSectionWindow.Show();
                    subjectSectionWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error opening Subject Section Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSectionAssign_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole == 4)
            {
                System.Windows.MessageBox.Show("Your account status does not allow access to Section Assign.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (sectionAssignWindow == null || !IsWindowOpen(sectionAssignWindow))
                {
                    sectionAssignWindow = new frmClass_SubjectWiseClassAssign(_uID);
                    sectionAssignWindow.Closed += (s, args) => { sectionAssignWindow = null; };
                    sectionAssignWindow.Show();
                }
                else
                {
                    if (sectionAssignWindow.WindowState == WindowState.Minimized)
                    {
                        sectionAssignWindow.WindowState = WindowState.Normal;
                    }
                    sectionAssignWindow.Show();
                    sectionAssignWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error opening Section Assign Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnVideoUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (videoUploadWindow == null || !IsWindowOpen(videoUploadWindow))
                {
                    videoUploadWindow = new frmClass_ClassVideoText(_uID);
                    videoUploadWindow.Closed += (s, args) => { videoUploadWindow = null; };
                    videoUploadWindow.Show();
                }
                else
                {
                    if (videoUploadWindow.WindowState == WindowState.Minimized)
                    {
                        videoUploadWindow.WindowState = WindowState.Normal;
                    }
                    videoUploadWindow.Show();
                    videoUploadWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error opening Video Upload Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnMessage_Click(object sender, RoutedEventArgs e)
        {
            if (_userRole == 4)
            {
                System.Windows.MessageBox.Show("Your account status does not allow access to Messages.", "Restricted", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (chatWin == null || !IsWindowOpen(chatWin))
                {
                    chatWin = new ChatBox(_uID);
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
                System.Windows.MessageBox.Show("Error opening Message Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}