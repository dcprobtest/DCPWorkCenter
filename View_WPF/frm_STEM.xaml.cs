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
// আপনার প্রজেক্টের স্ট্রাকচার অনুযায়ী সঠিক নেমস্পেসটি নিচে দেওয়া হলো
using ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frm_STEM.xaml
    /// </summary>
    public partial class frm_STEM : Window
    {
        // রিসিভ করা ইউজার আইডি রাখার জন্য ভেরিয়েবল
        private string _adminId;

        public frm_STEM()
        {
            InitializeComponent();
        }

        // Main_Adminfrm থেকে UID রিসিভ করার জন্য এই কনস্ট্রাক্টরটি ব্যবহার হবে
        public frm_STEM(string uid)
        {
            InitializeComponent();
            _adminId = uid;

            // ViewModel সেট করার সমাধান:
            // নিশ্চিত করুন যে ModelView_WPF প্রজেক্টটি View_WPF প্রজেক্টের Reference-এ যুক্ত আছে
            STEMViewModel vm = new STEMViewModel();
            vm.AdminID = _adminId;
            this.DataContext = vm;
        }

        // উইন্ডো বন্ধ করার জন্য এক্সিট বাটন মেথড
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // উইন্ডো বন্ধ করার জন্য
        }
    }
}