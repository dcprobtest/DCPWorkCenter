using Microsoft.Maui.Controls;
using ModelView_MAUI;

namespace View_MAUI
{
    public partial class frmAssignmentScrum : ContentPage
    {
        private string _loggedInUID = string.Empty;

        public frmAssignmentScrum()
        {
            InitializeComponent();
            // Initialize ViewModel without UID if needed
            BindingContext = new AssignmentScrumViewModel();
        }

        public frmAssignmentScrum(string uid)
        {
            InitializeComponent();
            _loggedInUID = uid;

            // Pass the received UID to the ViewModel via its constructor
            BindingContext = new AssignmentScrumViewModel(_loggedInUID);
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Logic handled here
        }
    }
}