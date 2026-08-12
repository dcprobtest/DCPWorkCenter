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
using View_WPF.ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmAdmin_Designation.xaml
    /// </summary>
    public partial class frmAdmin_Designation : Window
    {
        public frmAdmin_Designation()
        {
            InitializeComponent();
            this.DataContext = new DesignationViewModel();
        }

        public frmAdmin_Designation(string uid)
        {
            InitializeComponent();
            this.DataContext = new DesignationViewModel(uid);
        }
    }
}