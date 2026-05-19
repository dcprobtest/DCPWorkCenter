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
using ModelView_WPF; // ViewModel প্রজেক্টের নেমস্পেসটি যুক্ত করা হলো

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmAccIncSub.xaml
    /// </summary>
    public partial class frmAccIncSub : Window
    {
        public frmAccIncSub()
        {
            InitializeComponent();

            // ViewModel-এর একটি নতুন ইন্সট্যান্স তৈরি করে DataContext-এ অ্যাসাইন করা হলো
            this.DataContext = new AccIncSubViewModel();
        }

        // Main_Accountsfrm থেকে uid রিসিভ করার জন্য নতুন কনস্ট্রাক্টর
        public frmAccIncSub(string uid)
        {
            InitializeComponent();

            // ViewModel-এর ইন্সট্যান্সে uid পাস করে DataContext-এ অ্যাসাইন করা হলো
            this.DataContext = new AccIncSubViewModel(uid);
        }
    }
}