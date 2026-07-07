using Microsoft.Maui.Controls;

namespace View_MAUI
{
    public partial class frmAccEmpSalary : ContentPage
    {
        public frmAccEmpSalary(string uid)
        {
            InitializeComponent();

            // Pass the received uid to the ViewModel
            // Assuming your ViewModel is assigned to the BindingContext
            this.BindingContext = new AccEmpSalaryViewModel(uid);
        }
    }
}