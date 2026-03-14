using System;

namespace View_WPF.Model
{
    public class EmpInfoModel
    {
        public string UID { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }  
        public string Country { get; set; } 
        public string Phone { get; set; }
        public string Email { get; set; }
        public string PrimaryID { get; set; }
        public string SecondID { get; set; }
        public string DeptCode { get; set; }
        public string DesigCode { get; set; }
        public DateTime? DOB { get; set; } = DateTime.Now;
        public DateTime? EntryDate { get; set; } = DateTime.Now;
        public DateTime? StartDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; } = DateTime.Now;
        public string Activate { get; set; }
    }
}