using System;
using System.Windows;
using System.Windows.Controls;

namespace View_WPF
{
    /// <summary>
    /// Interaction logic for frm_OfferLetter.xaml
    /// </summary>
    public partial class frm_OfferLetter : Window
    {
        public frm_OfferLetter()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Email বাটন এ ক্লিক করলে এই মেথডটি কাজ করবে।
        /// </summary>
        private void EmailButton_Click(object sender, RoutedEventArgs e)
        {
            // এখানে আপনি আপনার ইমেইল পাঠানোর আসল লজিক (যেমন: SmtpClient বা Process.Start) যোগ করতে পারেন।
            // বর্তমানে বাটনটি ক্লিক করলে কোনো পপআপ আসবে না।
        }

        private void CreateDocument_Click(object sender, RoutedEventArgs e)
        {
            // 'Create Document' বাটনের জন্য লজিক এখানে লিখুন।
        }
    }
}