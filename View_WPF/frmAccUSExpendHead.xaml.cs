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
using ModelView_WPF; // Imported to access AccUSExpendHeadViewModel

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmAccUSExpendHead.xaml
    /// </summary>
    public partial class frmAccUSExpendHead : Window
    {
        public frmAccUSExpendHead()
        {
            InitializeComponent();

            // Binding the ViewModel to the View's DataContext
            this.DataContext = new AccUSExpendHeadViewModel();
        }
    }
}