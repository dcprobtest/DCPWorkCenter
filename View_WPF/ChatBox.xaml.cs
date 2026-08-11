using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModelView_WPF;

namespace View_WPF
{
    public partial class ChatBox : Window
    {
        private ChatBotViewModel _viewModel;
        private string _uid;

        public ChatBox()
        {
            InitializeComponent();
            _viewModel = new ChatBotViewModel();
            this.DataContext = _viewModel;
            ChatListBox.ItemsSource = _viewModel.Messages;

            _viewModel.Messages.CollectionChanged += (s, e) =>
            {
                ChatScrollViewer.ScrollToEnd();
            };
        }

        public ChatBox(string uid)
        {
            InitializeComponent();
            _uid = uid;

            _viewModel = new ChatBotViewModel(_uid);

            this.DataContext = _viewModel;
            ChatListBox.ItemsSource = _viewModel.Messages;

            _viewModel.Messages.CollectionChanged += (s, e) =>
            {
                ChatScrollViewer.ScrollToEnd();
            };
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputTextBox.Text))
            {
                await _viewModel.AddMessage(InputTextBox.Text, true);
                InputTextBox.Clear();
                ChatScrollViewer.ScrollToEnd();
            }
        }

        private void InputTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
            {
                SendButton_Click(null, null);
                e.Handled = true;
            }
        }

        private void ChatScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0)
            {
                ChatScrollViewer.ScrollToBottom();
            }
        }
    }
}