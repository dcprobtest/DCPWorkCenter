using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model_WPF
{
    public class ClassFocusMonitorModel
    {
        public string UID { get; set; }
        public string SubjectCode { get; set; }
        public int SessionID { get; set; }
        public string SectionID { get; set; }
        public DateTime? StudyDate { get; set; }
        public TimeSpan? EventTime { get; set; }
        public string MonitorTypeID { get; set; }
        public string SlideBarPercent { get; set; }
        public TimeSpan? SlideBarTime { get; set; }
    }
}