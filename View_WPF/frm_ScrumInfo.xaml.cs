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
using ModelView_WPF; // Added namespace to reference ScrumInfoViewModel

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frm_ScrumInfo.xaml
    /// </summary>
    public partial class frm_ScrumInfo : Window
    {
        public frm_ScrumInfo()
        {
            InitializeComponent();

            // Set the DataContext to establish connection with the ViewModel
            this.DataContext = new ScrumInfoViewModel();
        }
    }
}