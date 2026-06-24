using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ModelView_WPF
{
    // ViewModel implementation for ChatBox
    public class ChatBotViewModel : INotifyPropertyChanged
    {
        private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:7073/api/") };
        public ObservableCollection<ChatMessageViewModel> Messages { get; set; }

        // Field to store the received UID
        private string _uid;

        // Default constructor
        public ChatBotViewModel()
        {
            Messages = new ObservableCollection<ChatMessageViewModel>();
        }

        // Constructor to receive UID and load history automatically
        public ChatBotViewModel(string uid)
        {
            _uid = uid;
            Messages = new ObservableCollection<ChatMessageViewModel>();

            // Trigger history loading
            _ = LoadChatHistory();
        }

        // Method to load chat history from database via API
        public async Task LoadChatHistory()
        {
            try
            {
                var response = await _httpClient.GetAsync($"ChatBot/{_uid}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var history = JsonSerializer.Deserialize<List<ChatMessageViewModel>>(json, options);

                    if (history != null)
                    {
                        Messages.Clear();
                        foreach (var msg in history)
                        {
                            // Ensure IsMyMessage is set correctly based on UID match
                            // Assuming msg.Uid is available in your database model
                            Messages.Add(msg);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading history: " + ex.Message);
            }
        }

        // Add a new message to the list and send to API
        public async Task AddMessage(string text, bool isMyMessage, string status = "Sent", string dateSeparator = "")
        {
            // 1. Add to UI
            Messages.Add(new ChatMessageViewModel
            {
                Message = text,
                IsMyMessage = isMyMessage,
                StatusText = status,
                DateSeparator = dateSeparator
            });

            // 2. Send to API (if it's my message)
            if (isMyMessage)
            {
                try
                {
                    // Include the received UID in the payload to be saved in the database
                    var chatData = new { uid = _uid, chat = text };
                    string json = JsonSerializer.Serialize(chatData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    await _httpClient.PostAsync("ChatBot", content);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("API Error: " + ex.Message);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // Helper class to represent individual message properties
    public class ChatMessageViewModel
    {
        // Map the 'chat' JSON field from your API to the 'Message' property
        [JsonPropertyName("chat")]
        public string Message { get; set; }

        public bool IsMyMessage { get; set; }
        public string StatusText { get; set; }
        public string DateSeparator { get; set; }

        public Visibility DateVisibility => string.IsNullOrEmpty(DateSeparator) ? Visibility.Collapsed : Visibility.Visible;
        public HorizontalAlignment Alignment => IsMyMessage ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        public Brush BubbleColor => IsMyMessage ?
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6264A7")) :
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EDEDED"));
        public Brush TextColor => IsMyMessage ? Brushes.White : Brushes.Black;
        public CornerRadius BubbleRadius => IsMyMessage ?
            new CornerRadius(16, 16, 4, 16) :
            new CornerRadius(16, 16, 16, 4);
    }
}