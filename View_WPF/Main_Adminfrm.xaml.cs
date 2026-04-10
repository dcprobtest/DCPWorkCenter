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

// এরর সমাধানের জন্য নিচের Alias-টি যোগ করা হলো
using MessageBox = System.Windows.MessageBox;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for Main_Adminfrm.xaml
    /// </summary>
    public partial class Main_Adminfrm : Window
    {
        // লগইন করা ইউজার আইডি এবং নাম ধরে রাখার জন্য প্রাইভেট ভেরিয়েবল
        private string _uid;
        private string _userName;

        // ডিফল্ট কনস্ট্রাক্টর
        public Main_Adminfrm()
        {
            InitializeComponent();
        }

        // MainWindow থেকে UID এবং Name রিসিভ করার জন্য এই কনস্ট্রাক্টরটি ব্যবহার হবে
        public Main_Adminfrm(string uid, string name)
        {
            InitializeComponent();
            _uid = uid;
            _userName = name; // প্যারামিটার থেকে আসা নাম সেভ করে রাখা হলো
        }

        // User Table বাটনের ক্লিক ইভেন্ট
        private void BtnUserTable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // frmAdmin_User উইন্ডোটি ওপেন করা হচ্ছে
                frmAdmin_User userWin = new frmAdmin_User();
                userWin.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening User Table: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Create Docx বাটনের ক্লিক ইভেন্ট
        private void BtnCreateDocx_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // এখানে _uid এর পাশাপাশি _userName ও পাঠানো হচ্ছে
                frm_createdocx docxWin = new frm_createdocx(_uid, _userName);
                docxWin.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error opening Document Interface: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}