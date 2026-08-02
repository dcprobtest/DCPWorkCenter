using System.Windows;
using View_WPF.ModelView_WPF;

namespace View_WPF
{
    public partial class frmAdmin_EmpInfo : Window
    {
        public frmAdmin_EmpInfo()
        {
            InitializeComponent();
            this.DataContext = new EmpInfoViewModel();
        }

        public frmAdmin_EmpInfo(string uid)
        {
            InitializeComponent();
            this.DataContext = new EmpInfoViewModel(uid);
        }
    }
}