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
// ViewModel-এর নেমস্পেসটি যুক্ত করা হয়েছে
using View_WPF.ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmClass_SubjectWiseClassAssign.xaml
    /// </summary>
    public partial class frmClass_SubjectWiseClassAssign : Window
    {
        public frmClass_SubjectWiseClassAssign()
        {
            InitializeComponent();

            // ভিউ-এর সাথে SubjectWiseSectionAssignViewModel কানেক্ট করা হলো
            this.DataContext = new SubjectWiseSectionAssignViewModel();
        }

        // Main_Classfrm থেকে আসা UID রিসিভ করার জন্য নতুন কনস্ট্রাক্টর
        public frmClass_SubjectWiseClassAssign(string uid)
        {
            InitializeComponent();

            // ViewModel-এর অবজেক্ট তৈরি করা হচ্ছে
            var viewModel = new SubjectWiseSectionAssignViewModel();

            // ViewModel-এর ভেতরে EntryByUid প্রোপার্টিতে রিসিভ করা UID সেট করে দেওয়া হচ্ছে
            viewModel.EntryByUid = uid;

            // ভিউ-এর DataContext হিসেবে আপডেট করা ViewModel সেট করা হলো
            this.DataContext = viewModel;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}