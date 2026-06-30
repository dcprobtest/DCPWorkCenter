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
        }

        public frmAssignmentScrum(string uid)
        {
            InitializeComponent();
            _loggedInUID = uid;
        }

        private void Entry_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Logic handled here
        }
    }
}