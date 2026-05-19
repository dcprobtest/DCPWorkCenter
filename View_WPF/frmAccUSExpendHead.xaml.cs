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
        // Variable to store the logged-in user's UID
        private string _loggedInUID = string.Empty;

        public frmAccUSExpendHead()
        {
            InitializeComponent();

            // Binding the ViewModel to the View's DataContext
            this.DataContext = new AccUSExpendHeadViewModel();
        }

        // Parameterized constructor to receive UID from Main_frmUSAccounts window
        public frmAccUSExpendHead(string uid)
        {
            InitializeComponent();
            _loggedInUID = uid; // Setting the passed UID to the private variable

            // Binding the ViewModel to the View's DataContext and passing the UID
            this.DataContext = new AccUSExpendHeadViewModel(uid);
        }
    }
}