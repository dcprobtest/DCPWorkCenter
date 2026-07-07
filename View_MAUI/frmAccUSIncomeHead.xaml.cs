using System;
using Microsoft.Maui.Controls;

namespace View_MAUI
{
    public partial class frmAccUSIncomeHead : ContentPage
    {
        public string LoggedInUserId { get; private set; }

        public frmAccUSIncomeHead(string userId)
        {
            InitializeComponent();

            LoggedInUserId = userId;

            // Pass the userId to the ViewModel
            this.BindingContext = new AccUSIncomeHeadViewModel(LoggedInUserId);
        }

        private async void Exit_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}