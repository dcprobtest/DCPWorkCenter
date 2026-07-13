using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.AspNetCore.SignalR.Client;

namespace ModelView_MAUI
{
    // মডেল ক্লাসগুলো এখানে রাখা হয়েছে (প্রয়োজনে আলাদা ফাইলে নিতে পারেন)
    public class ParticipantModel : INotifyPropertyChanged
    {
        private string _name;
        private bool _isCameraOn = true;

        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
        public bool IsCameraOn { get => _isCameraOn; set { _isCameraOn = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class ChatBubbleModel
    {
        public string Sender { get; set; }
        public string Message { get; set; }
        public Microsoft.Maui.Primitives.LayoutAlignment Alignment { get; set; } // MAUI Alignment
        public string BubbleColor { get; set; }
    }

    public class VideoConferenceViewModel : INotifyPropertyChanged
    {
        private HubConnection _connection;
        private string _chatInput;

        public string ChatInput
        {
            get => _chatInput;
            set { _chatInput = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ChatBubbleModel> ChatMessages { get; set; } = new();
        public ObservableCollection<ParticipantModel> Participants { get; set; } = new();

        public ICommand SendMessageCommand { get; }
        public ICommand LeaveSessionCommand { get; }
        public ICommand ToggleMicCommand { get; }
        public ICommand ToggleCamCommand { get; }

        public VideoConferenceViewModel()
        {
            ChatMessages.Add(new ChatBubbleModel { Message = "System: Connecting to server...", Alignment = Microsoft.Maui.Primitives.LayoutAlignment.Center, BubbleColor = "#333333" });
            Participants.Add(new ParticipantModel { Name = "You (Owner)" });

            InitializeSignalR();

            SendMessageCommand = new Command(ExecuteSendMessage);
            LeaveSessionCommand = new Command(ExecuteLeaveSession);
            ToggleMicCommand = new Command(ExecuteToggleMic);
            ToggleCamCommand = new Command(ExecuteToggleCam);
        }

        private async void InitializeSignalR()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl("http://74.208.227.170:8050/conferenceHub")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ChatMessages.Add(new ChatBubbleModel { Sender = user, Message = message, Alignment = Microsoft.Maui.Primitives.LayoutAlignment.Start, BubbleColor = "#444444" });
                });
            });

            _connection.On<string>("UserJoined", (userName) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Participants.Add(new ParticipantModel { Name = userName });
                    ChatMessages.Add(new ChatBubbleModel { Message = $"System: {userName} joined.", Alignment = Microsoft.Maui.Primitives.LayoutAlignment.Center, BubbleColor = "#333333" });
                });
            });

            try
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("JoinMeeting", "Room101", "UID123", "User_" + new Random().Next(100, 999), "Mobile Device");
            }
            catch (Exception ex)
            {
                ChatMessages.Add(new ChatBubbleModel { Message = $"System: Connection Failed! {ex.Message}", Alignment = Microsoft.Maui.Primitives.LayoutAlignment.Center, BubbleColor = "#B22222" });
            }
        }

        private async void ExecuteSendMessage()
        {
            if (!string.IsNullOrWhiteSpace(ChatInput) && _connection?.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("SendMessage", "Room101", "You", ChatInput);
                ChatMessages.Add(new ChatBubbleModel { Sender = "You", Message = ChatInput, Alignment = Microsoft.Maui.Primitives.LayoutAlignment.End, BubbleColor = "#1A73E8" });
                ChatInput = string.Empty;
            }
        }

        private async void ExecuteLeaveSession()
        {
            bool answer = await Shell.Current.DisplayAlert("Leave Session", "Are you sure you want to leave?", "Yes", "No");
            if (answer)
            {
                if (_connection != null) await _connection.StopAsync();
                await Shell.Current.Navigation.PopAsync(); // MAUI-তে স্ক্রিন ক্লোজ করার উপায়
            }
        }

        private void ExecuteToggleMic() { /* Mic Logic */ }

        private void ExecuteToggleCam(object obj)
        {
            if (obj is bool isCamOn && Participants.Count > 0)
                Participants[0].IsCameraOn = isCamOn;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}