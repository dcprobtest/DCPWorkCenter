using System;
using System.Windows;
using View_WPF.ModelView_WPF; // আপনার দেওয়া ViewModel-এর সঠিক নেমস্পেস

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmAdmin_Department.xaml
    /// </summary>
    public partial class frmAdmin_Department : Window
    {
        public frmAdmin_Department()
        {
            InitializeComponent();

            // আপনার দেওয়া DepartmentViewModel ক্লাসটিকে এই উইন্ডোর ডাটা সোর্স হিসেবে সেট করা হলো
            // এর ফলে XAML-এর {Binding Dept.DeptCode} ইত্যাদি কাজ করবে
            this.DataContext = new DepartmentViewModel();
        }
    }
}