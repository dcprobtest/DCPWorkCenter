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
using ModelView_WPF; // Adding ViewModel namespace

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmAccUSIncSub.xaml
    /// </summary>
    public partial class frmAccUSIncSub : Window
    {
        // Variable to store the logged-in user's UID
        private string _loggedInUID = string.Empty;

        // Default constructor
        public frmAccUSIncSub()
        {
            InitializeComponent();

            // Setting Connection/DataContext with ViewModel
            this.DataContext = new AccUSIncSubViewModel();
        }

        // Parameterized constructor to receive UID from Main_frmUSAccounts window
        public frmAccUSIncSub(string uid)
        {
            InitializeComponent();
            _loggedInUID = uid; // Setting the passed UID to the private variable

            // Setting DataContext and passing the UID to the ViewModel constructor
            this.DataContext = new AccUSIncSubViewModel(_loggedInUID);
        }
    }
}