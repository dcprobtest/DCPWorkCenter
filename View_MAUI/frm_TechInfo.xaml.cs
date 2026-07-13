using ModelView_MAUI;
namespace View_MAUI;


public partial class frm_TechInfo : ContentPage
{
    private string _loggedInUserId;
    // Assuming you have a ViewModel property
    private TechInfoViewModel _viewModel;

    public frm_TechInfo(string uid)
    {
        InitializeComponent();
        _loggedInUserId = uid;

        // Initialize your ViewModel with the passed UID
        _viewModel = new TechInfoViewModel(_loggedInUserId);
        BindingContext = _viewModel;
    }
}