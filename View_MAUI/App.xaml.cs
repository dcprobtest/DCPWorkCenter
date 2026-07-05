using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace View_MAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // --- MediaElement Handler Registration ---
            // MauiProgram.cs ফাইল না থাকলে রানটাইমে মিডিয়া প্লেয়ারের হ্যান্ডলার রেজিস্টার করার সঠিক নিয়ম:
            Microsoft.Maui.Handlers.ElementHandler.ElementMapper.Add("MediaElement", (handler, view) =>
            {
                if (view is CommunityToolkit.Maui.Views.MediaElement)
                {
                    // মিডিয়া এলিমেন্ট যাতে ক্রাশ না করে তার হ্যান্ডলার ম্যাপিং এখানে ব্যাকআপ করা হলো
                }
            });
            // ----------------------------------------

            // অ্যাপ্লিকেশন রান করার সাথে সাথে প্রথম স্ক্রিন হিসেবে Login পেজটি NavigationPage এর সাথে ওপেন হবে
            MainPage = new NavigationPage(new Login());
        }
    }
}