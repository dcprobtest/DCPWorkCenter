using View_MAUI.ViewModel_MAUI;

namespace View_MAUI.View_MAUI
{
    public partial class ClassVideoTextView : ContentPage
    {
        public ClassVideoTextView()
        {
            InitializeComponent();

            // এখানে ডামি ইউজার আইডি (UID) হিসেবে "Admin01" পাঠানো হয়েছে, আপনার প্রয়োজনমতো চেঞ্জ করতে পারেন
            BindingContext = new ClassVideoTextViewModel("Admin01");
        }

        private void InitializeComponent()
        {
            throw new NotImplementedException();
        }
    }
}