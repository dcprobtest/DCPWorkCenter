using ModelView_MAUI;
namespace View_MAUI;


public partial class frmClass_SubjectWiseClassAssign : ContentPage
{
    // Property to hold the user ID
    public string ClassUserId { get; private set; }

    public frmClass_SubjectWiseClassAssign(string uid)
    {
        InitializeComponent();

        // Assign the received uid
        ClassUserId = uid;

        // Pass the uid to the ViewModel
        BindingContext = new SubjectWiseSectionAssignViewModel(ClassUserId);
    }
}