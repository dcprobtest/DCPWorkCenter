using System;
using System.Windows;
using System.Windows.Controls;

namespace View_WPF
{
    public partial class Main_Classfrm : Window
    {
        private string _uID;

        public Main_Classfrm()
        {
            InitializeComponent();
        }

        public Main_Classfrm(string uid)
        {
            InitializeComponent();
            _uID = uid;
        }

        private void btnClassForms_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("You are currently in the Course & Content Management form.");
        }

        private void btnCourseSubject_Click(object sender, RoutedEventArgs e)
        {
            frmClass_ClassSubject courseSubjectWindow = new frmClass_ClassSubject(_uID);
            courseSubjectWindow.Show();
        }

        private void btnSubjectSection_Click(object sender, RoutedEventArgs e)
        {
            frm_ClassSection subjectSectionWindow = new frm_ClassSection(_uID);
            subjectSectionWindow.Show();
        }

        private void btnSectionAssign_Click(object sender, RoutedEventArgs e)
        {
            frmClass_SubjectWiseClassAssign sectionAssignWindow = new frmClass_SubjectWiseClassAssign(_uID);
            sectionAssignWindow.Show();
        }

        private void btnVideoUpload_Click(object sender, RoutedEventArgs e)
        {
            frmClass_ClassVideoText videoUploadWindow = new frmClass_ClassVideoText(_uID);
            videoUploadWindow.Show();
        }

        private void BtnMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ChatBox chatWin = new ChatBox(_uID);
                chatWin.Show();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error opening Message Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}