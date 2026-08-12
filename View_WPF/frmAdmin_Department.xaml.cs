using System;
using System.Windows;
using View_WPF.ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmAdmin_Department.xaml
    /// </summary>
    public partial class frmAdmin_Department : Window
    {
        public frmAdmin_Department()
        {
            InitializeComponent();
            this.DataContext = new DepartmentViewModel();
        }

        public frmAdmin_Department(string uid)
        {
            InitializeComponent();
            this.DataContext = new DepartmentViewModel(uid);
        }
    }
}