using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModelView_WPF;

namespace View_WPF
{
    public partial class ChatBox : Window
    {
        // Initialize the ViewModel
        private ChatBotViewModel _viewModel;

        // Variable to store the received UID
        private string _uid;

        // Default constructor
        public ChatBox()
        {
            InitializeComponent();
            _viewModel = new ChatBotViewModel();
            this.DataContext = _viewModel;
            ChatListBox.ItemsSource = _viewModel.Messages;
        }

        // Updated constructor to receive and pass UID to ViewModel
        public ChatBox(string uid)
        {
            InitializeComponent();
            _uid = uid;

            // Initialize ViewModel with the UID
            _viewModel = new ChatBotViewModel(_uid);

            // Set the DataContext for data binding and set the ItemsSource
            this.DataContext = _viewModel;
            ChatListBox.ItemsSource = _viewModel.Messages;
        }

        // Handle the Send button click event - Changed to async
        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                // Add the message to the ViewModel collection and await the API call
                await _viewModel.AddMessage(InputTextBox.Text, true);

                // Clear the input box after sending
                InputTextBox.Clear();
            }
        }

        // Handle the Enter key to send messages
        // Explicitly using System.Windows.Input.KeyEventArgs to resolve ambiguity
        private void InputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                SendButton_Click(null, null);
                e.Handled = true;
            }
        }

        // Auto-scroll logic when new messages are added
        private void ChatScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Only scroll to bottom if the content height changes
            if (e.ExtentHeightChange != 0)
            {
                ChatScrollViewer.ScrollToBottom();
            }
        }
    }
}