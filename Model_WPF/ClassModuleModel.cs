using System;

namespace Model_WPF
{
    public class ClassModuleModel
    {
        
        public string CourseCode { get; set; } = string.Empty;

        
        public string? CourseDesc { get; set; }

        
        public DateTime? EntryDate { get; set; } = DateTime.Now;

        
        public DateTime? StartDate { get; set; } = DateTime.Now;

        public DateTime? EndDate { get; set; } = DateTime.Now.AddMonths(3);

        
        
        public string Activate { get; set; } = "1";
    }
}