using System;

namespace Model_WPF
{
    public class ClassModuleModel
    {
        // আপনার ডাটাবেজ টেবিল অনুযায়ী CourseCode
        public string CourseCode { get; set; } = string.Empty;

        // কোর্সের বিস্তারিত বিবরণ
        public string? CourseDesc { get; set; }

        // ডাটা এন্ট্রির তারিখ
        public DateTime? EntryDate { get; set; } = DateTime.Now;

        // কোর্স শুরু হওয়ার তারিখ
        public DateTime? StartDate { get; set; } = DateTime.Now;

        // কোর্স শেষ হওয়ার তারিখ
        public DateTime? EndDate { get; set; } = DateTime.Now.AddMonths(3);

        // স্ট্যাটাস: '1' (Active) অথবা '0' (Inactive) 
        // আপনি চাইলে আপনার আগের লজিক অনুযায়ী 'Y' বা 'N' ও ব্যবহার করতে পারেন
        public string Activate { get; set; } = "1";
    }
}