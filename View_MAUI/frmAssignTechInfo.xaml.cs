using Microsoft.Maui.Controls;

namespace View_MAUI
{
    public partial class frmAssignTechInfo : ContentPage
    {
        private string _loggedInUserId;

        public frmAssignTechInfo(string uid)
        {
            InitializeComponent();
            _loggedInUserId = uid;

            // Initialize ViewModel with the passed UID and assign to BindingContext
            this.BindingContext = new AssignTechInfoViewModel(_loggedInUserId);
        }
    }
}