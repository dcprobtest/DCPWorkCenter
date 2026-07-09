using System;

namespace Model_MAUI
{
    public class ChatBotModel
    {
        // Primary key to uniquely identify each message
        public int AssignID { get; set; }

        // Stores the exact time when the message was sent
        public DateTime EntryDate { get; set; }

        // Identifier for the user who sent the message
        public string UID { get; set; }

        // The actual text content of the chat message
        public string Chat { get; set; }
    }
}