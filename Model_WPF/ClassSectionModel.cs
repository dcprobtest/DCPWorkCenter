using System;

namespace Model_WPF
{
    public class ClassSectionModel
    {
        public string SectionID { get; set; } = string.Empty;

        public string? SubjectCode { get; set; }

        public string? SectionDesc { get; set; }

        public DateTime? EntryDate { get; set; } = DateTime.Now;
        public DateTime? StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; } = DateTime.Now;

        public string? EntryByUID { get; set; }

        public DateTime? LastUpdate { get; set; } = DateTime.Now;

        public string? Activate { get; set; } = "0";

        public bool IsActiveBool
        {
            get => Activate == "1" || Activate == "True" || Activate == "true";
            set => Activate = value ? "1" : "0";
        }
    }
}