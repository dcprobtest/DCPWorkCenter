using System;
using System.Windows;

// কনফ্লিক্ট দূর করার জন্য Alias ব্যবহার করা হলো
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // লগইন করা ইউজারের আইডি ধরে রাখার জন্য ভেরিয়েবল
        private string _loggedInUID;

        // ডিফল্ট কনস্ট্রাক্টর (যদি প্রয়োজন হয়)
        public MainWindow()
        {
            InitializeComponent();
        }

        // লগইন উইন্ডো থেকে UID সহ কল করার জন্য এই কনস্ট্রাক্টরটি ব্যবহার হবে
        public MainWindow(string uid)
        {
            InitializeComponent();
            _loggedInUID = uid;
        }

        // ভিডিও ক্লাস (Class Player) বাটনের ক্লিক ইভেন্ট
        private void btnVideoClass_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // এখানে _loggedInUID প্যারামিটার হিসেবে frmClass_ClassPlayer-এ পাঠানো হচ্ছে
                // যাতে এই UID অনুযায়ী স্ক্রিনশট ফোল্ডার তৈরি হতে পারে
                frmClass_ClassPlayer classPlayer = new frmClass_ClassPlayer(_loggedInUID);
                classPlayer.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ভিডিও ক্লাস ওপেন করতে সমস্যা হচ্ছে: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ফাইল ম্যানেজার (DCP Document Manager) বাটনের ক্লিক ইভেন্ট
        private void btnFileManager_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // frmAdmin_DocInfo উইন্ডোটি ওপেন করার সময় UID প্যারামিটার হিসেবে পাঠানো হচ্ছে
                frmAdmin_DocInfo docManager = new frmAdmin_DocInfo(_loggedInUID);
                docManager.Show();

                // যদি চান ফাইল ম্যানেজার ওপেন হলে মেইন উইন্ডো বন্ধ হয়ে যাবে, তবে নিচের লাইনটি ব্যবহার করুন:
                //this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ফাইল ম্যানেজার ওপেন করতে সমস্যা হচ্ছে: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // অন্যান্য বাটনের কাজ এখানে করতে পারেন
        }
    }
}