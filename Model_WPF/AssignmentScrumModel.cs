using System;

namespace Model_WPF
{
    public class AssignmentScrumModel
    {
        public int AssignID { get; set; }
        public string ScrumID { get; set; }
        public string AssignByUID { get; set; }
        public string AssignForUID { get; set; }
        public string StartStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public string ProgressStatus { get; set; }
        public DateTime? ProgressDate { get; set; }
        public string CompleteStatus { get; set; }
        public DateTime? CompleteDate { get; set; }
        public string AssignByDesc { get; set; }
        public string AssignForDesc { get; set; }
        public string Activate { get; set; }
    }
}