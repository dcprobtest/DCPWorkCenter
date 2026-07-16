using System;

namespace Model_WPF
{
    public class ClassWiseExamBatchModel
    {
        // Corresponds to BatchID char(3)
        public string BatchID { get; set; }

        // Corresponds to BatchDate datetime (Checked/Allow Nulls)
        public DateTime? BatchDate { get; set; }

        // Corresponds to Activate char(1) (Checked/Allow Nulls)
        public string Activate { get; set; }
    }
}