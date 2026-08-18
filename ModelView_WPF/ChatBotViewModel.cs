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
using System.Diagnostics;
using System.Windows.Input;
using Model_WPF;
using ModelView_WPF.Helpers;

namespace ModelView_WPF
{
    public class ChatBotViewModel : INotifyPropertyChanged
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public ObservableCollection<ChatMessageViewModel> Messages { get; set; }

        private string _uid;

        private bool _isReplying;
        public bool IsReplying
        {
            get => _isReplying;
            set { _isReplying = value; OnPropertyChanged(); OnPropertyChanged("ReplyContainerVisibility"); }
        }

        private string _replyingToMessage;
        public string ReplyingToMessage
        {
            get => _replyingToMessage;
            set { _replyingToMessage = value; OnPropertyChanged(); }
        }

        public Visibility ReplyContainerVisibility => IsReplying ? Visibility.Visible : Visibility.Collapsed;

        public ICommand CancelReplyCommand => new RelayCommand(o =>
        {
            IsReplying = false;
            ReplyingToMessage = string.Empty;
        });

        public ChatBotViewModel()
        {
            Messages = new ObservableCollection<ChatMessageViewModel>();

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            var client = new HttpClient(handler) { BaseAddress = new Uri(ApiSettings.BaseUrl) };

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri(ApiSettings.BaseUrl);
            }
        }

        public ChatBotViewModel(string uid) : this()
        {
            _uid = uid;
            _ = LoadChatHistory();
        }

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
                            var msgVm = new ChatMessageViewModel
                            {
                                Id = msg.AssignID, // AssignID ব্যবহার করা হলো
                                Message = msg.Chat,
                                IsMyMessage = isMine,
                                StatusText = "",
                                DateSeparator = "",
                                SenderInfo = isMine ? "You" : msg.SenderName,
                                ReplyText = null
                            };

                            msgVm.OnReplyRequested = (m) =>
                            {
                                IsReplying = true;
                                ReplyingToMessage = m.Message;
                            };

                            msgVm.OnDeleteForMeRequested = (m) =>
                            {
                                Messages.Remove(m);
                            };

                            msgVm.OnDeleteForEveryoneRequested = async (m) =>
                            {
                                Messages.Remove(m);
                                if (m.Id > 0)
                                {
                                    try
                                    {
                                        await _httpClient.DeleteAsync($"{ApiSettings.ChatBotBaseUrl}/{m.Id}");
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Delete API Error: " + ex.Message);
                                    }
                                }
                            };

                            Messages.Add(msgVm);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading history: " + ex.Message);
            }
        }

        public async Task AddMessage(string text, bool isMyMessage, string status = "Sent", string dateSeparator = "")
        {
            string currentReplyText = IsReplying ? ReplyingToMessage : null;

            var newMsgVm = new ChatMessageViewModel
            {
                Message = text,
                IsMyMessage = isMyMessage,
                StatusText = status,
                DateSeparator = dateSeparator,
                SenderInfo = "You",
                ReplyText = currentReplyText
            };

            // Reset reply state after sending
            IsReplying = false;
            ReplyingToMessage = string.Empty;

            newMsgVm.OnReplyRequested = (m) =>
            {
                IsReplying = true;
                ReplyingToMessage = m.Message;
            };

            newMsgVm.OnDeleteForMeRequested = (m) =>
            {
                Messages.Remove(m);
            };

            newMsgVm.OnDeleteForEveryoneRequested = async (m) =>
            {
                Messages.Remove(m);
                if (m.Id > 0)
                {
                    try
                    {
                        await _httpClient.DeleteAsync($"{ApiSettings.ChatBotBaseUrl}/{m.Id}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Delete API Error: " + ex.Message);
                    }
                }
            };

            Messages.Add(newMsgVm);

            if (isMyMessage)
            {
                try
                {
                    var chatData = new { uid = _uid, chat = text };
                    string json = JsonSerializer.Serialize(chatData);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(ApiSettings.ChatBotBaseUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseString = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var createdMsg = JsonSerializer.Deserialize<ChatMessageDto>(responseString, options);
                        if (createdMsg != null && createdMsg.AssignID > 0)
                        {
                            newMsgVm.Id = createdMsg.AssignID; // ব্যাকএন্ড থেকে আসা আসল AssignID এখানে সেট করা হলো
                        }
                    }
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

    public class ChatMessageDto
    {
        [JsonPropertyName("assignID")]
        public int AssignID { get; set; } // ব্যাকএন্ডের AssignID এর সাথে ম্যাপ করা হলো

        [JsonPropertyName("uid")]
        public string UID { get; set; }

        [JsonPropertyName("chat")]
        public string Chat { get; set; }

        [JsonPropertyName("senderName")]
        public string SenderName { get; set; }

        [JsonPropertyName("entryDate")]
        public DateTime EntryDate { get; set; }
    }

    public class ChatMessageViewModel
    {
        [JsonPropertyName("assignID")]
        public int Id { get; set; } // ভিউ-মডেলের Id প্রপার্টি

        [JsonPropertyName("chat")]
        public string Message { get; set; }

        public bool IsMyMessage { get; set; }
        public string StatusText { get; set; }
        public string DateSeparator { get; set; }
        public string SenderInfo { get; set; }

        // Property to hold the replied message text inside the bubble
        private string _replyText;
        public string ReplyText
        {
            get => _replyText;
            set { _replyText = value; }
        }

        public Visibility ReplyPreviewVisibility => string.IsNullOrEmpty(ReplyText) ? Visibility.Collapsed : Visibility.Visible;

        public Action<ChatMessageViewModel> OnReplyRequested { get; set; }
        public Action<ChatMessageViewModel> OnDeleteForMeRequested { get; set; }
        public Action<ChatMessageViewModel> OnDeleteForEveryoneRequested { get; set; }

        public ICommand OpenLinkCommand => new RelayCommand(o =>
        {
            string url = o as string;
            if (!string.IsNullOrEmpty(url))
            {
                try
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error opening link: " + ex.Message);
                }
            }
        });

        public ICommand ReplyCommand => new RelayCommand(o =>
        {
            OnReplyRequested?.Invoke(this);
        });

        public ICommand CopyCommand => new RelayCommand(o =>
        {
            if (!string.IsNullOrEmpty(Message))
            {
                Clipboard.SetText(Message);
            }
        });

        public ICommand DeleteForMeCommand => new RelayCommand(o =>
        {
            OnDeleteForMeRequested?.Invoke(this);
        });

        public ICommand DeleteForEveryoneCommand => new RelayCommand(o =>
        {
            OnDeleteForEveryoneRequested?.Invoke(this);
        });

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