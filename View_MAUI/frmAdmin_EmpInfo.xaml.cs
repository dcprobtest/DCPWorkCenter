using View_MAUI.ViewModel_MAUI;

namespace View_MAUI;

public partial class frmAdmin_EmpInfo : ContentPage
{
    // Property to store the received user ID
    public string AdminUserId { get; private set; }

    public frmAdmin_EmpInfo(string uid)
    {
        InitializeComponent();

        // Assign the received uid
        AdminUserId = uid;

        // Initialize the ViewModel and pass the uid to it
        //this.BindingContext = new EmpInfoViewModel(AdminUserId);
    }
}