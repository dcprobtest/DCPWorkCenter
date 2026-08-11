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
using Model_WPF;
using ModelView_WPF.Helpers;

namespace ModelView_WPF
{
    // ViewModel implementation for ChatBox
    public class ChatBotViewModel : INotifyPropertyChanged
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public ObservableCollection<ChatMessageViewModel> Messages { get; set; }

        // Field to store the received UID
        private string _uid;

        // Default constructor
        public ChatBotViewModel()
        {
            Messages = new ObservableCollection<ChatMessageViewModel>();

            // Localhost self-signed SSL certificate error bypass handler (Fixes: SSL Connection Could Not Be Established)
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            // Re-initialize HttpClient with the handler and ApiSettings.BaseUrl
            var client = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            // Assign to static field if needed or use instance-level setup. Since _httpClient is static, let's assign its base address or update implementation safely:
            // (To keep logic identical, we configure the base address using ApiSettings)
            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(ApiSettings.BaseUrl);
            }
        }

        // Constructor to receive UID and load history automatically
        public ChatBotViewModel(string uid) : this()
        {
            _uid = uid;

            // Trigger history loading
            _ = LoadChatHistory();
        }

        // Method to load chat history from database via API
        public async Task LoadChatHistory()
        {
            try
            {
                var response = await _httpClient.GetAsync(ApiSettings.ChatBotBaseUrl);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var history = JsonSerializer.Deserialize<List<ChatMessageDto>>(json, options);

                    if (history != null)
                    {
                        Messages.Clear();
                        foreach (var msg in history)
                        {
                            bool isMine = (msg.UID == _uid);
                            Messages.Add(new ChatMessageViewModel
                            {
                                Message = msg.Chat,
                                IsMyMessage = isMine,
                                StatusText = "",
                                DateSeparator = "",
                                // নিজের মেসেজ হলে "You" আর অন্যের মেসেজ হলে API থেকে আসা ইউজারের আসল নাম দেখাবে
                                SenderInfo = isMine ? "You" : msg.SenderName
                            });
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
                DateSeparator = dateSeparator,
                SenderInfo = "You" // নিজের পাঠানো মেসেজের ওপর "You" দেখাবে
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

                    await _httpClient.PostAsync(ApiSettings.ChatBotBaseUrl, content);
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

    // DTO to match API response containing SenderName and UID
    public class ChatMessageDto
    {
        [JsonPropertyName("uid")]
        public string UID { get; set; }

        [JsonPropertyName("chat")]
        public string Chat { get; set; }

        [JsonPropertyName("senderName")]
        public string SenderName { get; set; }

        [JsonPropertyName("entryDate")]
        public DateTime EntryDate { get; set; }
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
        public string SenderInfo { get; set; }

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