using View_MAUI.ViewModel_MAUI;

namespace View_MAUI
{
    public partial class ClassVideoTextView : ContentPage
    {
        // Property to hold the user ID
        public string ClassUserId { get; private set; }

        public ClassVideoTextView(string uid)
        {
            InitializeComponent();

            // Assign the received uid
            ClassUserId = uid;

            // Pass the uid to the ViewModel
            BindingContext = new ClassVideoTextViewModel(ClassUserId);
        }
    }
}