using View_MAUI.ViewModel_MAUI;

namespace View_MAUI
{
    public partial class frmAdmin_Department : ContentPage
    {
        public frmAdmin_Department()
        {
            InitializeComponent();

            // ভিউ-মডেলকে পেজের BindingContext হিসেবে সেট করা হলো
            this.BindingContext = new DepartmentViewModel();
        }
    }
}