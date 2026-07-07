using System;
using Microsoft.Maui.Controls;

namespace View_MAUI
{
    public partial class frmAccUSExpendHead : ContentPage
    {
        public string LoggedInUserId { get; private set; }

        public frmAccUSExpendHead(string userId)
        {
            InitializeComponent();

            LoggedInUserId = userId;

            // Pass the userId to the ViewModel
            this.BindingContext = new AccUSExpendHeadViewModel(LoggedInUserId);
        }

        private async void Exit_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}