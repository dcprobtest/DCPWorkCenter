using System;

namespace Model_WPF
{
    public class ClassWiseExamBatchStudentModel
    {
        // CWBSID nchar(10) - Unchecked (Nullable)
        public string CWBSID { get; set; }

        // BatchID char(3) - Checked (Required)
        public string BatchID { get; set; }

        // BatchDate datetime - Checked (Required)
        public DateTime BatchDate { get; set; }

        // Activate char(1) - Checked (Required)
        public string Activate { get; set; }
    }
}