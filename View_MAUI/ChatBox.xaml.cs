using System;
using Microsoft.Maui.Controls;

namespace View_MAUI
{
    public partial class ChatBox : ContentPage
    {
        private ChatBotViewModel _viewModel;

        public ChatBox()
        {
            InitializeComponent();

            // টেস্ট করার সুবিধার্থে একটি ডেমো UID (যেমন: "U001") পাস করা হয়েছে
            // বাস্তব ক্ষেত্রে লগইন করা ইউজারের আইডি এখানে পাস করতে পারেন
            _viewModel = new ChatBotViewModel("U001");
            this.BindingContext = _viewModel;
        }

        private async void SendButton_Clicked(object sender, EventArgs e)
        {
            string messageText = InputEntry.Text;

            if (!string.IsNullOrWhiteSpace(messageText))
            {
                // ১. ভিউমডেলের মেথড কল করে মেসেজ লিস্টে যোগ করা এবং এপিআই-তে পাঠানো
                await _viewModel.AddMessage(messageText, true);

                // ২. মেসেজ পাঠানোর পর ইনপুট বক্স খালি করা
                InputEntry.Text = string.Empty;

                // ৩. নতুন মেসেজ যোগ হলে স্ক্রোল করে অটোমেটিক একদম নিচে নিয়ে যাওয়া
                if (_viewModel.ChatMessages.Count > 0)
                {
                    ChatCollectionView.ScrollTo(_viewModel.ChatMessages.Count - 1);
                }
            }
        }
    }
}