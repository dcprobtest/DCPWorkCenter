using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data; // BindingExpression এর জন্য প্রয়োজন
// আপনার ViewModel এর সঠিক Namespace
using View_WPF.ModelView_WPF;

// কনফ্লিক্ট দূর করার জন্য Alias ব্যবহার করা হলো
using TextBox = System.Windows.Controls.TextBox;
using Button = System.Windows.Controls.Button;

namespace View_WPF
{
    public partial class frmAdmin_DocInfo : Window
    {
        private UserDocInfoViewModel _viewModel;

        // MainWindow থেকে আসা UID গ্রহণ করার কনস্ট্রাক্টর
        public frmAdmin_DocInfo(string uid)
        {
            InitializeComponent();

            // ViewModel এর অবজেক্ট তৈরি করা
            _viewModel = new UserDocInfoViewModel();

            // উইন্ডোর DataContext সেট করা
            this.DataContext = _viewModel;

            // উইন্ডোটি পুরোপুরি লোড হওয়ার পর ডাটা সেট করার জন্য ইভেন্ট হ্যান্ডলার
            this.Loaded += (s, e) =>
            {
                if (!string.IsNullOrEmpty(uid))
                {
                    // ViewModel-এর UID প্রোপার্টিতে ভ্যালু সেট করা 
                    // এটি ভিউমডেলের ভেতর OnPropertyChanged ট্রিগার করবে
                    _viewModel.UID = uid;
                    _viewModel.UserID = uid;

                    // বাইন্ডিং যদি কোনো কারণে কাজ না করে তবে সরাসরি UI এলিমেন্ট আপডেট করা
                    UpdateUIDInUI(uid);
                }
            };
        }

        private void UpdateUIDInUI(string uid)
        {
            // ডাইনামিকভাবে UID প্রদর্শনের টেক্সটবক্সটি খোঁজা হচ্ছে
            // নোট: আপনার XAML এ যদি TextBox এর নাম x:Name="txtUID" দেওয়া থাকে তবে এটি কাজ করবে
            var txtBox = this.FindName("txtUID") as TextBox;

            if (txtBox != null)
            {
                // Dispatcher ব্যবহার করা হচ্ছে যাতে মেইন থ্রেডে টেক্সট আপডেট নিশ্চিত হয়
                this.Dispatcher.Invoke(() =>
                {
                    txtBox.Text = uid;
                    txtBox.IsReadOnly = true;

                    // নিশ্চিত করা যে বাইন্ডিং সোর্স আপডেট হয়েছে
                    BindingExpression binding = txtBox.GetBindingExpression(TextBox.TextProperty);
                    binding?.UpdateTarget();
                });
            }
        }

        // Plus Button Click Event Handler
        private void PlusButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn != null && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.DataContext = this.DataContext;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            // আপলোড লজিক ভিউমডেলের কমান্ডের মাধ্যমে হ্যান্ডেল হবে (ExecuteSave)
        }
    }
}