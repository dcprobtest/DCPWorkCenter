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
using ViewModel_WPF;

namespace View_WPF
{
    public partial class frmAccExpBub : Window
    {
        private readonly AccExpBubViewModel _viewModel;

        public frmAccExpBub()
        {
            InitializeComponent();

            _viewModel = new AccExpBubViewModel();

            this.DataContext = _viewModel;
        }

        public frmAccExpBub(string uid)
        {
            InitializeComponent();

            _viewModel = new AccExpBubViewModel(uid);

            this.DataContext = _viewModel;
        }
    }
}