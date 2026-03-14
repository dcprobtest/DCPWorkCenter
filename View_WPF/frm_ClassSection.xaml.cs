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

            // ViewModel সেট করা।
            if (this.DataContext == null)
            {
                this.DataContext = new ClassSectionViewModel();
            }
        }
    }
}