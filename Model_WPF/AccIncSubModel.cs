using System;

namespace Model_WPF
{
    public class AccIncSubModel
    {
        // Maps to IncSubCode char(4) - Unchecked (Required)
        private string _incSubCode = string.Empty;
        public string IncSubCode
        {
            get => _incSubCode;
            set => _incSubCode = value ?? string.Empty;
        }

        // Maps to IncCode char(2) - Checked (Nullable)
        public string? IncCode { get; set; }

        // Maps to IncDetail varchar(30) - Checked (Nullable)
        public string? IncDetail { get; set; }

        // Maps to UID varchar(5) - Checked (Nullable)
        public string? UID { get; set; }

        // Maps to EntryDate datetime - Checked (Nullable)
        public DateTime? EntryDate { get; set; }
    }
}