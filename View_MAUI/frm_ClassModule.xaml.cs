using Microsoft.Maui.Controls;
using ModelView_MAUI;

namespace View_MAUI
{
    public partial class frm_ClassModule : ContentPage
    {
        public frm_ClassModule()
        {
            InitializeComponent();

            // XAML UI এর সাথে ভিউমডেল যুক্ত করার স্টেটমেন্ট
            this.BindingContext = new ClassModuleViewModel();
        }
    }
}