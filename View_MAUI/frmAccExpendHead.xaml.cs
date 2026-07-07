using Microsoft.Maui.Controls;
using View_MAUI;

namespace View_MAUI;

public partial class frmAccExpendHead : ContentPage
{
    public frmAccExpendHead(string uid)
    {
        InitializeComponent();

        // Pass the received uid to the ViewModel
        this.BindingContext = new AccExpendHeadViewModel(uid);
    }
}