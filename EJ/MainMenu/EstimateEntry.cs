using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EJ.MainMenu
{
    public class EstimateEntry
    {
        public string UserName { get; set; }
        public string GroupName { get; set; }
        public string SubjectName { get; set; }
        public DateTime Date { get; set; }
        public int Estimate { get; set; }
        public DateTime LessonDate { get; set; }
        public string Description { get; set; }
    }
}
