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
    /// Interaction logic for frmAssignmentScrum.xaml
    /// </summary>
    public partial class frmAssignmentScrum : Window
    {
        private string _loggedInUID = string.Empty;

        public frmAssignmentScrum()
        {
            InitializeComponent();
            this.DataContext = new AssignmentScrumViewModel();
        }

        // Overloaded constructor to receive the UID from Main_ScrumWindow and pass it to ViewModel
        public frmAssignmentScrum(string uid)
        {
            InitializeComponent();
            _loggedInUID = uid;
            this.DataContext = new AssignmentScrumViewModel(uid);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Event handler to prevent CS1061 compilation error
        }
    }
}