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

// আপনার প্রোজেক্টের নতুন নেমস্পেস অনুযায়ী ViewModel ইমপোর্ট করা হয়েছে
using ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmClass_ClassSubject.xaml
    /// </summary>
    public partial class frmClass_ClassSubject : Window
    {
        public frmClass_ClassSubject()
        {
            // এই মেথডটি XAML ডিজাইনের সাথে C# কোডকে যুক্ত করে
            InitializeComponent();

            // ভিউ-এর সাথে ClassSubjectViewModel কানেক্ট করা হলো
            // যদি XAML-এ DataContext দেওয়া থাকে, তবে এটি রানটাইমে সেট হবে
            if (this.DataContext == null)
            {
                this.DataContext = new ClassSubjectViewModel();
            }
        }
    }
}