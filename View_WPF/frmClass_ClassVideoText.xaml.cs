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
using View_WPF.ModelView_WPF; // ViewModel এর নেমস্পেস নিশ্চিত করা হয়েছে

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmClass_ClassVideoText.xaml
    /// </summary>
    public partial class frmClass_ClassVideoText : Window
    {
        public frmClass_ClassVideoText()
        {
            InitializeComponent();

            // ViewModel-কে View-এর সাথে কানেক্ট করার জন্য DataContext সেট করা হয়েছে
            // এটি রানটাইমে XAML বাইন্ডিংগুলোকে ডাটা সরবরাহ করবে
            this.DataContext = new ClassVideoTextViewModel();
        }
    }
}