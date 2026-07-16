using System;
using System.Windows;
using ModelView_WPF; // আপনার ViewModel যে নেমস্পেসে আছে সেটি ব্যবহার করুন

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmClassWiseExam.xaml
    /// </summary>
    public partial class frmClassWiseExam : Window
    {
        public frmClassWiseExam()
        {
            InitializeComponent();

            // এখানে ViewModel-টিকে DataContext হিসেবে সেট করে দেওয়া হয়েছে
            // এটিই XAML এবং ViewModel-এর মধ্যে সেতুবন্ধন তৈরি করবে
            this.DataContext = new ClassWiseExamViewModel();
        }
    }
}