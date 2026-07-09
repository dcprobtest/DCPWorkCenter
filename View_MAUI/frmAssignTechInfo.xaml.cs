using Microsoft.Maui.Controls;

namespace View_MAUI
{
    public partial class frmAssignTechInfo : ContentPage
    {
        public frmAssignTechInfo()
        {
            InitializeComponent();

            // ভিউমডেল অবজেক্ট তৈরি করে এই পেজের ডাটা বাইন্ডিং কন্টেক্সটে অ্যাসাইন করা হলো
            this.BindingContext = new AssignTechInfoViewModel();
        }
    }
}