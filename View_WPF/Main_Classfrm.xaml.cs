using System;
using System.Windows;
using System.Windows.Controls;

namespace View_WPF
{
    public partial class Main_Classfrm : Window
    {
        // রিসিভ করা UID ধরে রাখার জন্য ভেরিয়েবল
        private string _uID;

        public Main_Classfrm()
        {
            InitializeComponent();
        }

        // MainWindow থেকে আসা UID রিসিভ করার জন্য নতুন কনস্ট্রাক্টর
        public Main_Classfrm(string uid)
        {
            InitializeComponent();
            _uID = uid;
        }

        // ক্লাস ফর্মের ভেতরকার ক্লিক ইভেন্ট হ্যান্ডলার
        private void btnClassForms_Click(object sender, RoutedEventArgs e)
        {
            // যেহেতু ইউজার বর্তমানে এই উইন্ডোতেই আছে
            System.Windows.MessageBox.Show("আপনি বর্তমানে কোর্স ও কন্টেন্ট ম্যানেজমেন্ট ফর্মে আছেন।");
        }

        // কোর্স সাবজেক্ট বাটনের ক্লিক ইভেন্ট হ্যান্ডলার
        private void btnCourseSubject_Click(object sender, RoutedEventArgs e)
        {
            // frmClass_ClassSubject উইন্ডোটি তৈরি করা হচ্ছে এবং _uID পাস করা হচ্ছে
            frmClass_ClassSubject courseSubjectWindow = new frmClass_ClassSubject(_uID);
            courseSubjectWindow.Show();
        }

        // সাবজেক্ট সেকশন বাটনের ক্লিক ইভেন্ট হ্যান্ডলার
        private void btnSubjectSection_Click(object sender, RoutedEventArgs e)
        {
            // frm_ClassSection উইন্ডোটি তৈরি করা হচ্ছে এবং _uID পাস করা হচ্ছে
            frm_ClassSection subjectSectionWindow = new frm_ClassSection(_uID);
            subjectSectionWindow.Show();
        }

        // সেকশন অ্যাসাইন বাটনের ক্লিক ইভেন্ট হ্যান্ডলার
        private void btnSectionAssign_Click(object sender, RoutedEventArgs e)
        {
            // এখন _uID পাস করে ফর্মটি ওপেন করা হচ্ছে
            frmClass_SubjectWiseClassAssign sectionAssignWindow = new frmClass_SubjectWiseClassAssign(_uID);
            sectionAssignWindow.Show();
        }

        // ভিডিও আপলোড বাটনের ক্লিক ইভেন্ট হ্যান্ডলার
        private void btnVideoUpload_Click(object sender, RoutedEventArgs e)
        {
            // frmClass_ClassVideoText উইন্ডোটি তৈরি করা হচ্ছে এবং _uID পাস করা হচ্ছে
            frmClass_ClassVideoText videoUploadWindow = new frmClass_ClassVideoText(_uID);
            videoUploadWindow.Show();
        }
    }
}