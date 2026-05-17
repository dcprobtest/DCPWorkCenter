using System;
using System.Windows;
using ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmAccEmpSalary.xaml
    /// </summary>
    public partial class frmAccEmpSalary : Window
    {
        // রিসিভ করা ইউজার আইডি স্টোর করার জন্য ভেরিয়েবল
        private string _userUID;

        // ডিফল্ট কনস্ট্রাক্টর
        public frmAccEmpSalary()
        {
            InitializeComponent();
        }

        // Main_Accountsfrm থেকে পাঠানো UID রিসিভ করার জন্য কনস্ট্রাক্টর
        public frmAccEmpSalary(string uid)
        {
            InitializeComponent();

            // পাঠানো আইডিটি ভেরিয়েবলে সেট করা হলো
            this._userUID = uid;

            // প্রয়োজনে চেক করার জন্য টাইটেলে আইডি দেখাতে পারেন
            // this.Title = "Employee Salary - User: " + _userUID;
        }
    }
}