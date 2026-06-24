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
using View_WPF.ModelView_WPF; // Added to reference the TechInfoViewModel namespace

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frm_TechInfo.xaml
    /// </summary>
    public partial class frm_TechInfo : Window
    {
        private string _loggedInUID = string.Empty;

        public frm_TechInfo()
        {
            InitializeComponent();

            // Connects the View with the ViewModel for data binding and commands
            this.DataContext = new TechInfoViewModel();
        }

        // Overloaded constructor to receive the UID from Main_ScrumWindow and pass it to ViewModel
        public frm_TechInfo(string uid)
        {
            InitializeComponent();
            _loggedInUID = uid;

            // Passing the received UID to the TechInfoViewModel
            this.DataContext = new TechInfoViewModel(uid);
        }
    }
}