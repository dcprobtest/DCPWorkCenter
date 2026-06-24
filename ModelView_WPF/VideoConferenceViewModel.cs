using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace ModelView_WPF
{
    public class ParticipantModel : INotifyPropertyChanged
    {
        private string _name;
        private bool _isCameraOn = true;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public bool IsCameraOn
        {
            get => _isCameraOn;
            set { _isCameraOn = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class ChatBubbleModel
    {
        public string Sender { get; set; }
        public string Message { get; set; }
        public HorizontalAlignment Alignment { get; set; }
        public string BubbleColor { get; set; }
    }

    public class VideoConferenceViewModel : INotifyPropertyChanged
    {
        private HubConnection _connection;
        private string _chatInput;

        public string ChatInput
        {
            get => _chatInput;
            set
            {
                if (_chatInput != value)
                {
                    _chatInput = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ChatBubbleModel> ChatMessages { get; set; }
        public ObservableCollection<ParticipantModel> Participants { get; set; }

        public ICommand SendMessageCommand { get; }
        public ICommand LeaveSessionCommand { get; }
        public ICommand ToggleMicCommand { get; }
        public ICommand ToggleCamCommand { get; }

        public VideoConferenceViewModel()
        {
            ChatMessages = new ObservableCollection<ChatBubbleModel>();
            Participants = new ObservableCollection<ParticipantModel>();

            ChatMessages.Add(new ChatBubbleModel { Message = "System: Connecting to server...", Alignment = HorizontalAlignment.Center, BubbleColor = "#333333" });

            Participants.Add(new ParticipantModel { Name = "You (Owner)" });

            InitializeSignalR();

            SendMessageCommand = new RelayCommand(ExecuteSendMessage);
            LeaveSessionCommand = new RelayCommand(ExecuteLeaveSession);
            ToggleMicCommand = new RelayCommand(ExecuteToggleMic);
            ToggleCamCommand = new RelayCommand(ExecuteToggleCam);
        }

        private async void InitializeSignalR()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://74.208.227.170:8050//conferenceHub")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ChatMessages.Add(new ChatBubbleModel
                    {
                        Sender = user,
                        Message = message,
                        Alignment = HorizontalAlignment.Left,
                        BubbleColor = "#444444"
                    });
                });
            });

            _connection.On<string>("UserJoined", (userName) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Participants.Add(new ParticipantModel { Name = userName });
                    ChatMessages.Add(new ChatBubbleModel { Message = $"System: {userName} joined the meeting.", Alignment = HorizontalAlignment.Center, BubbleColor = "#333333" });
                });
            });

            try
            {
                // কানেকশন শুরু করার আগে এপিআই রান আছে কি না নিশ্চিত হোন
                await _connection.StartAsync();

                // হাব মেথড কল
                await _connection.InvokeAsync("JoinMeeting", "Room101", "UID123", "User_" + new Random().Next(100, 999), "Windows PC");

                ChatMessages.Add(new ChatBubbleModel { Message = "System: Connected to Conference.", Alignment = HorizontalAlignment.Center, BubbleColor = "#228B22" });
            }
            catch (Exception ex)
            {
                // এরর মেসেজটি চ্যাটে দেখাবে যাতে আপনি বুঝতে পারেন সমস্যা কোথায়
                ChatMessages.Add(new ChatBubbleModel { Message = $"System: Connection Failed! {ex.Message}", Alignment = HorizontalAlignment.Center, BubbleColor = "#B22222" });
            }
        }

        private async void ExecuteSendMessage(object obj)
        {
            if (!string.IsNullOrWhiteSpace(ChatInput) && _connection != null && _connection.State == HubConnectionState.Connected)
            {
                try
                {
                    await _connection.InvokeAsync("SendMessage", "Room101", "You", ChatInput);

                    ChatMessages.Add(new ChatBubbleModel
                    {
                        Sender = "You",
                        Message = ChatInput,
                        Alignment = HorizontalAlignment.Right,
                        BubbleColor = "#1A73E8"
                    });

                    ChatInput = string.Empty;
                }
                catch (Exception) { }
            }
        }

        private async void ExecuteLeaveSession(object obj)
        {
            var result = MessageBox.Show("Are you sure you want to leave the conference?", "Leave Session", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                if (_connection != null) await _connection.StopAsync();
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext == this) { window.Close(); break; }
                }
            }
        }

        private void ExecuteToggleMic(object obj) { /* Mic Logic */ }

        private void ExecuteToggleCam(object obj)
        {
            if (obj is bool isCamOn && Participants.Count > 0)
            {
                Participants[0].IsCameraOn = isCamOn;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}