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
using ModelView_WPF; // References the project/namespace containing the ViewModel

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmAccUSIncomeHead.xaml
    /// </summary>
    public partial class frmAccUSIncomeHead : Window
    {
        // Variable to store the logged-in user's UID
        private string _loggedInUID = string.Empty;

        // Default constructor
        public frmAccUSIncomeHead()
        {
            InitializeComponent();

            // Binding the ViewModel to the View's DataContext
            this.DataContext = new AccUSIncomeHeadViewModel();
        }

        // Parameterized constructor to receive UID from Main_frmUSAccounts window
        public frmAccUSIncomeHead(string uid)
        {
            InitializeComponent();
            _loggedInUID = uid; // Setting the passed UID to the private variable

            // Setting DataContext and passing the UID to the ViewModel constructor
            this.DataContext = new AccUSIncomeHeadViewModel(_loggedInUID);
        }
    }
}