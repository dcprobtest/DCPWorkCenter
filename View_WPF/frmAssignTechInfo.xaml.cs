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
using ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frmAssignTechInfo.xaml
    /// </summary>
    public partial class frmAssignTechInfo : Window
    {
        private string _loggedInUID = string.Empty;

        public frmAssignTechInfo()
        {
            InitializeComponent();

            // Initializing the ViewModel and assigning it to the DataContext of this Window
            this.DataContext = new AssignTechInfoViewModel();
        }

        // Overloaded constructor to receive the UID from Main_ScrumWindow and pass it to ViewModel
        public frmAssignTechInfo(string uid)
        {
            InitializeComponent();
            _loggedInUID = uid;
            this.DataContext = new AssignTechInfoViewModel(uid);
        }
    }
}