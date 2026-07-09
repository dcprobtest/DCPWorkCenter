using System;

namespace Model_MAUI
{
    public class TechInfoModel
    {
        // Required field (Unchecked in DB)
        public string TechId { get; set; }

        // Nullable fields (Checked in DB)
        public string TechDesc { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string EntryByUid { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string Activate { get; set; } = "N"; // Default database state

        // UI Wrapper property for WPF CheckBox binding
        public bool IsActivated
        {
            get => Activate == "Y";
            set => Activate = value ? "Y" : "N";
        }
    }
}