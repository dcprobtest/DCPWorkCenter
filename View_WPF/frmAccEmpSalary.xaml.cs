using System;
using System.Windows;
using ModelView_WPF;

namespace View_WPF
{
    public partial class frmAccEmpSalary : Window
    {
        private string _userUID;

        public frmAccEmpSalary()
        {
            InitializeComponent();
        }

        public frmAccEmpSalary(string uid)
        {
            InitializeComponent();

            this._userUID = uid;
        }
    }
}