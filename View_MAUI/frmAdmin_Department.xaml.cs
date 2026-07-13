using View_MAUI.ViewModel_MAUI;

namespace View_MAUI
{
    public partial class frmAdmin_Department : ContentPage
    {
        // Property to store the received user ID
        public string AdminUserId { get; private set; }

        public frmAdmin_Department(string uid)
        {
            InitializeComponent();

            // Assign the received uid
            AdminUserId = uid;

            // Initialize the ViewModel and pass the uid to it
            // Note: Ensure your DepartmentViewModel constructor is updated to accept the string parameter
            this.BindingContext = new DepartmentViewModel(AdminUserId);
        }
    }
}