using System;

namespace View_WPF.Model_MAUI
{
    public class DepartmentModel
    {
        public string DeptCode { get; set; }
        public string Description { get; set; }
        public string Responsiblity { get; set; } // Kept spelling as per your DB
        public DateTime? EntryDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Activate { get; set; } // "Y" or "N"
    }
}