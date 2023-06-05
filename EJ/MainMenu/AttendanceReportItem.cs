using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EJ.MainMenu
{
    public class AttendanceReportItem
    {
        public string SubjectName { get; set; }
        public Dictionary<DateTime, string> DateData { get; set; }
        public int UPCount { get; set; }
        public int NCount { get; set; }

        public AttendanceReportItem()
        {
            DateData = new Dictionary<DateTime, string>();
        }
    }
}
