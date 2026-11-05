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

        [JsonProperty("Activate")] 
        public string IsActive { get; set; }
    }
}