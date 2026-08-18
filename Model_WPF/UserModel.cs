using System;
using Newtonsoft.Json;

namespace View_WPF.Models
{
    public class UserModel
    {
        public string UID { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [JsonProperty("Activate")] // ডেটাবেজের কলাম নামের সাথে নিখুঁতভাবে ম্যাপ করার জন্য এটি জরুরি
        public string IsActive { get; set; }
    }
}