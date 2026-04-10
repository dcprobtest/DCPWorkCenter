using System;
using System.Windows;
using ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frm_ClassSection.xaml
    /// </summary>
    public partial class frm_ClassSection : Window
    {
        public frm_ClassSection()
        {
            // এটি ডিজাইনের সাথে কোডকে কানেক্ট করে।
            InitializeComponent();

            // ViewModel সেট করা (ডিফল্ট কনস্ট্রাক্টর কল হলে)
            if (this.DataContext == null)
            {
                this.DataContext = new ClassSectionViewModel(null);
            }
        }

        // নতুন কনস্ট্রাক্টর যা UID রিসিভ করবে এবং ViewModel-এ পাস করবে
        public frm_ClassSection(string uid)
        {
            InitializeComponent();

            // ViewModel সেট করা এবং UID পাস করা
            this.DataContext = new ClassSectionViewModel(uid);
        }
    }
}