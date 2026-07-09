using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Primary Key সেট করার জন্য নিচের namespace টি প্রয়োজন
using System.ComponentModel.DataAnnotations;

namespace Model_MAUI
{
    public class STEMModel
    {
        // TransID (varchar(11))
        [Key] // এটি যোগ করা হয়েছে কারণ TransID এখন Primary Key
        public string TransID { get; set; }

        // AdminUID (varchar(6))
        public string AdminUID { get; set; }

        // StudentUID (varchar(6))
        public string StudentUID { get; set; }

        // dYear (char(4))
        public string dYear { get; set; }

        // dMonth (varchar(2))
        public string dMonth { get; set; }

        // PayDate (date) 
        // Allow Nulls চেক করা আছে বলে DateTime? (Nullable) ব্যবহার করা হয়েছে
        public DateTime? PayDate { get; set; }

        // Hours (varchar(3))
        public string Hours { get; set; }

        // Rate (float)
        public double Rate { get; set; }

        // SUI (float)
        public double SUI { get; set; }

        // EmpTax (float)
        public double EmpTax { get; set; }

        // Total (float)
        public double Total { get; set; }

        // Paid (float)
        public double Paid { get; set; }
    }
}