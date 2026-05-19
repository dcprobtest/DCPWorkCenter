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
using ModelView_WPF; // References the ViewModel project namespace

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmAccUSExpBub.xaml
    /// </summary>
    public partial class frmAccUSExpBub : Window
    {
        // Variable to store the logged-in user's UID
        private string _loggedInUID;

        public frmAccUSExpBub()
        {
            InitializeComponent();

            // Connecting the View with the ViewModel via DataContext
            this.DataContext = new AccUSExpBubViewModel();
        }

        // Parameterized constructor to receive UID from Main_frmUSAccounts
        public frmAccUSExpBub(string uid)
        {
            InitializeComponent();
            _loggedInUID = uid; // Setting the passed UID to the private variable

            // Connecting the View with the ViewModel via DataContext
            this.DataContext = new AccUSExpBubViewModel(uid);

            // Optional: Setting the window title to display the logged-in user ID for tracking
            this.Title = $"Acc US Exp Bub - Logged in as: {uid}";
        }
    }
}