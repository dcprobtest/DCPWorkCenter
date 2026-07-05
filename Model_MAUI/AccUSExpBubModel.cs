using System;

namespace Model_MAUI
{
    public class AccUSExpBubModel
    {
        // Primary Key or Unique Identifiers (char(4) in database, cannot be null)
        public string ExpSubCode { get; set; }

        // Optional field for expense descriptions (varchar(30), allows null)
        public string ExpDetail { get; set; }

        // Code linking to main expense category (char(2), allows null)
        public string ExpCode { get; set; }

        // Stores the ID of the user who entered the record (varchar(5), allows null)
        public string UID { get; set; }

        // Stores the exact timestamp when the record was created (datetime, allows null)
        public DateTime? EntryDate { get; set; }
    }
}