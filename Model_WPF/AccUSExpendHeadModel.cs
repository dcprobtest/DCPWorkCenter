using System;

namespace Model_WPF
{
    public class AccUSExpendHeadModel
    {
        // ExpCode char(2) Unchecked (Primary Key বা Required ফিল্ড হতে পারে, তাই string)
        public string ExpCode { get; set; }

        // ExpDetail varchar(25) Checked (Null হতে পারে, তাই string)
        public string ExpDetail { get; set; }

        // UID varchar(5) Checked (Null হতে পারে, তাই string)
        public string UID { get; set; }

        // EntryDate datetime Checked (Null হতে পারে, তাই Nullable DateTime?)
        public DateTime? EntryDate { get; set; }
    }
}