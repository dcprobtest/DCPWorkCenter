using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace View_MAUI
{
    // ViewModel implementation for ChatBox
    public class ChatBotViewModel : INotifyPropertyChanged
    {
        // Bypass SSL policy handler can be added here if needed for localhost API
        private static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        })
        {
            BaseAddress = new Uri("https://localhost:7073/api/")
        };

        // Name matched with your XAML ItemsSource binding
        public ObservableCollection<ChatMessageViewModel> ChatMessages { get; set; }

        private string _uid;

        // Default constructor
        public ChatBotViewModel()
        {
            ChatMessages = new ObservableCollection<ChatMessageViewModel>();
        }

        // Constructor to receive UID and load history automatically
        public ChatBotViewModel(string uid)
        {
            _uid = uid;
            ChatMessages = new ObservableCollection<ChatMessageViewModel>();

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
                        ChatMessages.Clear();
                        foreach (var msg in history)
                        {
                            ChatMessages.Add(msg);
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
            ChatMessages.Add(new ChatMessageViewModel
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
        [JsonPropertyName("chat")]
        public string Message { get; set; }

        public bool IsMyMessage { get; set; }
        public string StatusText { get; set; }
        public string DateSeparator { get; set; }

        // MAUI uses boolean for IsVisible binding instead of Visibility enum
        public bool DateVisibility => !string.IsNullOrEmpty(DateSeparator);

        // WPF HorizontalAlignment converts to MAUI LayoutOptions
        public LayoutOptions Alignment => IsMyMessage ? LayoutOptions.End : LayoutOptions.Start;

        // Color properties using cross-platform Microsoft.Maui.Graphics.Color
        public Color BubbleColor => IsMyMessage ?
            Color.FromArgb("#6264A7") :
            Color.FromArgb("#EDEDED");

        public Color TextColor => IsMyMessage ? Colors.White : Colors.Black;
    }
}