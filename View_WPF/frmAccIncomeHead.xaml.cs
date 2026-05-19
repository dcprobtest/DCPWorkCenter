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
using ModelView_WPF; // References the ViewModel project namespace

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmAccIncomeHead.xaml
    /// </summary>
    public partial class frmAccIncomeHead : Window
    {
        public frmAccIncomeHead()
        {
            InitializeComponent();

            // Connects the View to the ViewModel via DataContext
            this.DataContext = new AccIncomeHeadViewModel();
        }

        // Main_Accountsfrm থেকে uid রিসিভ করার জন্য নতুন কনস্ট্রাক্টর
        public frmAccIncomeHead(string uid)
        {
            InitializeComponent();

            // ViewModel-এর ইন্সট্যান্সে uid পাস করে DataContext-এ অ্যাসাইন করা হলো
            this.DataContext = new AccIncomeHeadViewModel(uid);
        }
    }
}