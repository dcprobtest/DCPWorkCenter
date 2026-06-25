using Microsoft.Maui.Controls;

namespace View_MAUI
{
    public partial class frm_ClassSection : ContentPage
    {
        public frm_ClassSection()
        {
            InitializeComponent();
        }

        private void OnPointerEntered(object sender, PointerEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.ScaleTo(1.1, 150);
            }
        }

        private void OnPointerExited(object sender, PointerEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.ScaleTo(1.0, 150);
            }
        }
    }
}