namespace DCPWorkCenter_Api.Models
{
    public class UserDocRequest
    {
        public string? CandidateID { get; set; }
        public string? SelectedDocType { get; set; }
        public DateTime? JobStartDate { get; set; }
        public DateTime? JobEndDate { get; set; }
        public string? LoggedInUserID { get; set; }
        public string? Email { get; set; }
        public string? JobDesignation { get; set; }
        public string? JobResponsibility { get; set; }
        public string? HourlyRate { get; set; }
        public string? WeeklyHours { get; set; }
    }
}