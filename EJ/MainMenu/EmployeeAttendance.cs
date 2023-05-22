using System.Collections.Generic;

namespace EJ.MainMenu
{
    public class EmployeeAttendance
    {
        private readonly List<string> days = new List<string>(new string[31]);

        public string Name { get; set; }
        public string StudentId { get; set; }
        public List<string> Days => days;

        public int DisrespectfulReason
        {
            get
            {
                int count = 0;
                foreach (var day in days)
                {
                    if (day == "H")
                    {
                        count += 2;
                    }
                }
                return count;
            }
        }

        public int ValidReason
        {
            get
            {
                int count = 0;
                foreach (var day in days)
                {
                    if (day == "УП")
                    {
                        count += 2;
                    }
                }
                return count;
            }
        }
    }
}
