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

// আপনার ViewModel যে নেমস্পেসে আছে সেটি এখানে ইমপোর্ট করা হয়েছে
using ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frm_ClassModule.xaml
    /// </summary>
    public partial class frm_ClassModule : Window
    {
        public frm_ClassModule()
        {
            // এই মেথডটি XAML ডিজাইন এবং C# কোডকে কানেক্ট করে।
            InitializeComponent();

            // ভিউ-এর সাথে ClassModuleViewModel কানেক্ট করা হলো।
            // যেহেতু আপনি XAML-এও DataContext দিয়েছেন, তাই নিচের কন্ডিশনটি দেওয়া নিরাপদ।
            if (this.DataContext == null)
            {
                this.DataContext = new ClassModuleViewModel();
            }
        }
    }
}