using System;

namespace Model_WPF
{
    public class AccIncomeHeadModel
    {
        // Maps to IncCode char(2) - Unchecked (Required)
        private string _incCode = string.Empty;
        public string IncCode
        {
            get => _incCode;
            set => _incCode = value ?? string.Empty;
        }

        // Maps to IncDetail varchar(25) - Checked (Nullable)
        public string IncDetail { get; set; }

        // Maps to UID varchar(5) - Checked (Nullable)
        public string UID { get; set; }

        // Maps to EntryDate datetime - Checked (Nullable)
        public DateTime? EntryDate { get; set; }
    }
}