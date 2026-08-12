using System;

namespace Model_WPF
{
    public class DesignationModel
    {
        public string DesigCode { get; set; }
        public string DesigName { get; set; }
        public string Description { get; set; }
        public string Responsiblity { get; set; }
        public DateTime? EntryDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string EntryByUid { get; set; } // এটি যুক্ত করা হয়েছে
        public DateTime? LastUpdate { get; set; } // এটি যুক্ত করা হয়েছে
        public string Activate { get; set; } // 'Y' or 'N'
    }
}