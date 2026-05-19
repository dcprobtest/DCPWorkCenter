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
using ViewModel_WPF; // ViewModel এর নেমস্পেসটি নিশ্চিত করুন

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmAccExpBub.xaml
    /// </summary>
    public partial class frmAccExpBub : Window
    {
        // ViewModel এর একটি রেফারেন্স রাখা ভালো প্র্যাকটিস
        private readonly AccExpBubViewModel _viewModel;

        public frmAccExpBub()
        {
            InitializeComponent();

            // ViewModel ইনিশিয়ালাইজ করা
            _viewModel = new AccExpBubViewModel();

            // এই উইন্ডোর DataContext হিসেবে ViewModel কে সেট করা
            // এটি করার ফলেই XAML এর Binding গুলো কাজ করবে
            this.DataContext = _viewModel;
        }

        // Main_Accountsfrm থেকে uid রিসিভ করার জন্য নতুন কনস্ট্রাক্টর
        public frmAccExpBub(string uid)
        {
            InitializeComponent();

            // ViewModel ইনিশিয়ালাইজ করার সময় uid পাস করা হলো
            _viewModel = new AccExpBubViewModel(uid);

            // DataContext সেট করা হলো
            this.DataContext = _viewModel;
        }
    }
}