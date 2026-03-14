namespace View_WPF.Model_WPF
{
    public class UserDocInfoModel
    {
        public string UserDocID { get; set; }
        public string UID { get; set; }
        public string DocType { get; set; }
        public string DocPath { get; set; }
        public DateTime EntryDate { get; set; } = DateTime.Now;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
        public string Activate { get; set; }
    }
}