using System;

namespace Model_WPF
{
    public class AssignTechInfoModel
    {
        // Primary Key - int, Unchecked (Not Null)
        public int AssignTechInfoID { get; set; }

        // int, Checked (Allow Nulls)
        public int? AssignID { get; set; }

        // varchar(5), Checked (Allow Nulls)
        public string ScrumID { get; set; }

        // varchar(6), Checked (Allow Nulls)
        public string EntryByUID { get; set; }

        // datetime, Checked (Allow Nulls)
        public DateTime? EntryDate { get; set; }

        // char(1), Checked (Allow Nulls)
        public string Activate { get; set; }
    }
}