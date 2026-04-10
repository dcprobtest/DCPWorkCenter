using System;
using System.Windows;

// কনফ্লিক্ট দূর করার জন্য Alias ব্যবহার করা হলো
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace View_WPF
{
    public partial class MainWindow : Window
    {
        // লগইন করা ইউজারের আইডি এবং নাম ধরে রাখার ভেরিয়েবল
        private string _loggedInUID;
        private string _loggedInName;

        public MainWindow()
        {
            InitializeComponent();
        }

        // লগইন উইন্ডো থেকে UID এবং Name রিসিভ করার কনস্ট্রাক্টর
        public MainWindow(string uid, string name)
        {
            InitializeComponent();
            _loggedInUID = uid;
            _loggedInName = name;
        }

        // ভিডিও ক্লাস (Class Player) বাটনের ক্লিক ইভেন্ট
        private void btnVideoClass_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frmClass_ClassPlayer classPlayer = new frmClass_ClassPlayer(_loggedInUID);
                classPlayer.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ভিডিও ক্লাস ওপেন করতে সমস্যা হচ্ছে: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ফাইল ম্যানেজার বাটনের ক্লিক ইভেন্ট
        private void btnFileManager_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                frmAdmin_DocInfo docManager = new frmAdmin_DocInfo(_loggedInUID);
                docManager.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ফাইল ম্যানেজার ওপেন করতে সমস্যা হচ্ছে: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Admin Forms বাটনের ক্লিক ইভেন্ট
        private void btnAdminForms_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Main_Adminfrm adminForms = new Main_Adminfrm(_loggedInUID, _loggedInName);
                adminForms.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("এডমিন ফর্ম ওপেন করতে সমস্যা হচ্ছে: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Class Forms বাটনের ক্লিক ইভেন্ট (নতুন আপডেট)
        private void btnClassForms_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // এখানে _loggedInUID প্যারামিটার হিসেবে Main_Classfrm-এ পাঠানো হচ্ছে
                Main_Classfrm classForms = new Main_Classfrm(_loggedInUID);
                classForms.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ক্লাস ফর্ম ওপেন করতে সমস্যা হচ্ছে: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // অন্যান্য বাটনের কাজ
        }
    }
}