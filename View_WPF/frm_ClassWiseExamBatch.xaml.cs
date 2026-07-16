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
using ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frm_ClassWiseExamBatch.xaml
    /// </summary>
    public partial class frm_ClassWiseExamBatch : Window
    {
        public frm_ClassWiseExamBatch()
        {
            InitializeComponent();

            // Connect the ViewModel to the DataContext of the View
            this.DataContext = new ClassWiseExamBatchViewModel();
        }
    }
}