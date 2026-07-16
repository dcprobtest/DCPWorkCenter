using System;

namespace DCPWorkCenter.Model_WPF
{
    public class ClassWiseExamModel
    {
        public string MCQID { get; set; }
        public DateTime? EntryDate { get; set; }
        public string SectionID { get; set; }
        public string Question { get; set; }
        public string Ans1 { get; set; }
        public string Ans2 { get; set; }
        public string Ans3 { get; set; }
        public string Ans4 { get; set; }
        public string RightAns { get; set; }
        public string Activate { get; set; }
    }
}