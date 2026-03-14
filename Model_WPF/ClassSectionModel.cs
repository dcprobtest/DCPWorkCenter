using System;

namespace Model_WPF // আপনার প্রজেক্টের Folder গঠন অনুযায়ী নেমস্পেস
{
    public class ClassSectionModel
    {
        // SQL: varchar(6) - Primary Key
        // ছবির সাথে মিল রেখে ID বড় হাতের অক্ষর দেওয়া হয়েছে
        public string SectionID { get; set; } = string.Empty;

        // SQL: varchar(4) - Subject/Course Code
        public string? SubjectCode { get; set; }

        // SQL: varchar(200) - Section Description
        public string? SectionDesc { get; set; }

        // DateTime প্রপার্টিগুলো Nullable রাখা হয়েছে যাতে ডাটাবেসে null থাকলেও এরর না আসে
        public DateTime? EntryDate { get; set; } = DateTime.Now;
        public DateTime? StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; } = DateTime.Now;

        // ছবির সাথে মিল রেখে UID বড় হাতের অক্ষর দেওয়া হয়েছে
        public string? EntryByUID { get; set; }

        public DateTime? LastUpdate { get; set; } = DateTime.Now;

        // SQL: char(1) - "1" (Active) অথবা "0" (Inactive)
        public string? Activate { get; set; } = "0";
    }
}