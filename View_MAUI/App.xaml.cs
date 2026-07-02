using Microsoft.Maui.Controls;

namespace View_MAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // অ্যাপ্লিকেশন রান করার সাথে সাথে প্রথম স্ক্রিন হিসেবে Login পেজটি ওপেন হবে
            MainPage = new Login();
        }
    }
}