using System;

namespace Model_WPF
{
    public class AccExpendHeadModel
    {
        public string ExpCode { get; set; }     // char(2)
        public string ExpDetail { get; set; }   // varchar(25)
        public string UID { get; set; }         // varchar(5)
        public DateTime? EntryDate { get; set; } // datetime
    }
}