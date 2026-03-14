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
// ViewModel-এর নেমস্পেসটি যুক্ত করা হয়েছে
using View_WPF.ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmAdmin_DocInfo.xaml
    /// </summary>
    public partial class frmAdmin_DocInfo : Window
    {
        public frmAdmin_DocInfo()
        {
            InitializeComponent();

            // ভিউ-এর সাথে UserDocInfoViewModel কানেক্ট করা হলো
            this.DataContext = new UserDocInfoViewModel();
        }
    }
}