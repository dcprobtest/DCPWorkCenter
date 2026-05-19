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
// ViewModel এর নেমস্পেসটি এখানে যুক্ত করা হয়েছে
using ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmAccExpendHead.xaml
    /// </summary>
    public partial class frmAccExpendHead : Window
    {
        // ViewModel এর একটি রেফারেন্স রাখা ভালো প্র্যাকটিস
        private readonly AccExpendHeadViewModel _viewModel;

        public frmAccExpendHead()
        {
            InitializeComponent();

            // ViewModel ইনিশিয়ালাইজ করা হচ্ছে
            _viewModel = new AccExpendHeadViewModel();

            // উইন্ডোর DataContext হিসেবে ViewModel কে সেট করা হলো
            // এর মাধ্যমেই XAML এর সাথে C# কোডের বাইন্ডিং কাজ করবে
            this.DataContext = _viewModel;
        }

        // Main_Accountsfrm থেকে uid রিসিভ করার জন্য নতুন কনস্ট্রাক্টর
        public frmAccExpendHead(string uid)
        {
            InitializeComponent();

            // ViewModel ইনিশিয়ালাইজ করার সময় uid পাস করা হলো
            _viewModel = new AccExpendHeadViewModel(uid);

            // উইন্ডোর DataContext সেট করা হলো
            this.DataContext = _viewModel;
        }
    }
}