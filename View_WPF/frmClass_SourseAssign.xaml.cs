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
// ViewModel এর নেমস্পেসটি এখানে যুক্ত করা হয়েছে (আপনার প্রজেক্ট স্ট্রাকচার অনুযায়ী এটি চেক করে নিন)
using View_WPF.ViewModels;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmClass_SourseAssign.xaml
    /// </summary>
    public partial class frmClass_SourseAssign : Window
    {
        public frmClass_SourseAssign()
        {
            InitializeComponent();

            // ViewModel কে উইন্ডোর DataContext হিসেবে সেট করা হয়েছে
            // এর ফলে XAML এর প্রপার্টিগুলোর সাথে ViewModel এর ডাটা আদান-প্রদান শুরু হবে
            this.DataContext = new SourseAssignViewModel();
        }
    }
}