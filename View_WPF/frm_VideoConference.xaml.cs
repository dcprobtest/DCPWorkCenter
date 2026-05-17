using System;
using System.Collections.Specialized;
using System.Windows;
using ModelView_WPF;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frm_VideoConference.xaml
    /// </summary>
    public partial class frm_VideoConference : Window
    {
        public frm_VideoConference()
        {
            InitializeComponent();

            // ভিউমডেল অবজেক্ট ক্রিয়েশন
            var viewModel = new VideoConferenceViewModel();
            this.DataContext = viewModel;

            // ১ নম্বর রিকোয়ারমেন্ট: কালেকশনে মেসেজ অ্যাড হলেই চ্যাট স্ক্রোলভিউয়ার অটো নিচে নামার লজিক
            if (viewModel.ChatMessages != null)
            {
                viewModel.ChatMessages.CollectionChanged += ChatMessages_CollectionChanged;
            }
        }

        private void ChatMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // নতুন আইটেম অ্যাড হলে ব্যাকগ্রাউন্ড থ্রেড সেফটি বজায় রেখে স্ক্রোল ডাউন করবে
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                this.Dispatcher.Invoke(() =>
                {
                    ChatScrollViewer.ScrollToBottom();
                });
            }
        }
    }
}