using Microsoft.Maui.Controls;
using ModelView_MAUI;


namespace View_MAUI
{
    public partial class frm_ClassSection : ContentPage
    {
        // Property to hold the user ID
        public string ClassUserId { get; private set; }

        public frm_ClassSection(string uid)
        {
            InitializeComponent();

            // Assign the received uid
            ClassUserId = uid;

            // Pass the uid to the ViewModel
            BindingContext = new ClassSectionViewModel(ClassUserId);
        }

        // Triggered when the pointer enters the border
        private async void OnPointerEntered(object sender, PointerEventArgs e)
        {
            if (sender is VisualElement view)
            {
                await view.ScaleTo(1.03, 150, Easing.CubicOut);
            }
        }

        // Triggered when the pointer leaves the border
        private async void OnPointerExited(object sender, PointerEventArgs e)
        {
            if (sender is VisualElement view)
            {
                await view.ScaleTo(1.0, 150, Easing.CubicIn);
            }
        }
    }
}