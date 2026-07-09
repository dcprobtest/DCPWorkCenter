using System;

namespace View_WPF.Model_MAUI
{
    public class ClassVideoTextModel
    {
        // API এবং Database এর সাথে মিল রেখে string থেকে int করা হলো
        public int ClassID { get; set; }

        public string SectionID { get; set; }

        public string SectionDesc { get; set; }

        public string ClassContent { get; set; }

        // --- নতুন যোগ করা প্রপার্টি ---
        public string ClassPath { get; set; } // ভিডিওর লোকাল পাথ এখানে থাকবে

        public string EntryByUID { get; set; } // কে এন্ট্রি দিচ্ছে তার আইডি

        public DateTime? EntryDate { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? LastUpdate { get; set; } // ডাটাবেস ট্র্যাকিং এর জন্য

        // Database এ bit টাইপ হওয়ায় এখানে bool করা হলো যাতে JSON ঠিকমতো কনভার্ট হয়
        public bool Activate { get; set; }
    }
}