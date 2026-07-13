using Microsoft.Maui.Controls;
using View_MAUI.ViewModel_MAUI; // Assuming your ViewModel is in this namespace

namespace View_MAUI
{
    public partial class frmAdmin_Designation : ContentPage
    {
        // Property to store the received user ID
        public string AdminUserId { get; private set; }

        public frmAdmin_Designation(string uid)
        {
            InitializeComponent();

            // Assign the received uid
            AdminUserId = uid;

            // Initialize the ViewModel and pass the uid to it
            this.BindingContext = new DesignationViewModel(AdminUserId);
        }
    }
}