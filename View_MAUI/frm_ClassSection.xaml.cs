using Microsoft.Maui.Controls;

namespace ModelView_MAUI
{
    public partial class frm_ClassSection : ContentPage
    {
        public frm_ClassSection()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            throw new NotImplementedException();
        }

        // মাউস কার্ডের ওপর ঢুকলে (Hover Scale Up)
        private async void OnPointerEntered(object sender, PointerEventArgs e)
        {
            if (sender is VisualElement view)
            {
                await view.ScaleTo(1.03, 150, Easing.CubicOut);
            }
        }

        // মাউস কার্ড থেকে বের হয়ে গেলে (Scale Down to normal)
        private async void OnPointerExited(object sender, PointerEventArgs e)
        {
            if (sender is VisualElement view)
            {
                await view.ScaleTo(1.0, 150, Easing.CubicIn);
            }
        }
    }
}