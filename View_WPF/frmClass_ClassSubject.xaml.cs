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

// আপনার প্রোজেক্টের নতুন নেমস্পেস অনুযায়ী ViewModel ইমপোর্ট করা হয়েছে
using ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmClass_ClassSubject.xaml
    /// </summary>
    public partial class frmClass_ClassSubject : Window
    {
        // রিসিভ করা UID ধরে রাখার জন্য ভেরিয়েবল
        private string _uID;

        public frmClass_ClassSubject()
        {
            InitializeComponent();

            if (this.DataContext == null)
            {
                this.DataContext = new ClassSubjectViewModel();
            }
        }

        // নতুন কনস্ট্রাক্টর যা UID রিসিভ করবে এবং ViewModel-এ পাস করবে
        public frmClass_ClassSubject(string uid)
        {
            InitializeComponent();
            _uID = uid;

            // ভিউ-এর DataContext হিসেবে ViewModel সেট করা হচ্ছে এবং সেখানে UID পাস করা হচ্ছে
            this.DataContext = new ClassSubjectViewModel(_uID);
        }
    }
}