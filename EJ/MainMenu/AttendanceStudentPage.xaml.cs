using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EJ.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для AttendanceStudentPage.xaml
    /// </summary>
    public partial class AttendanceStudentPage : Page
    {
        private readonly BDEntities db = new BDEntities();
        public AttendanceStudentPage()
        {
            InitializeComponent();
            string userName = Application.Current.Properties["Name"] as string;
            NameTextBlock.Text = userName;
            GetGroupName();
        }

        private void GetGroupName()
        {
            int currentUser = (int)Application.Current.Properties["UserId"];

            using (var dbContext = new BDEntities()) // Замените YourDbContext на имя вашего контекста базы данных
            {
                var student = dbContext.Students.FirstOrDefault(s => s.UserId == currentUser);
                if (student != null)
                {
                    var groupId = student.GroupId;
                    var group = dbContext.Groups.FirstOrDefault(g => g.GroupId == groupId);
                    if (group != null)
                    {
                        string groupName = group.GroupName;
                        GroupsTextBlock.Text = groupName;
                    }
                }
            }

        }
        private void ToCreate_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate = StartOfPeriod.SelectedDate ?? DateTime.MinValue;
            DateTime endDate = EndOfPeriod.SelectedDate ?? DateTime.MaxValue;

            int studentId = GetStudentId();

            var query = from attendance in db.Attendance
                        join student in db.Students on attendance.StudentId equals student.StudentId
                        join subject in db.Subjects on attendance.SubjectId equals subject.SubjectId
                        where student.StudentId == studentId &&
                              attendance.Date >= startDate && attendance.Date <= endDate
                        select new
                        {
                            subject.SubjectName,
                            attendance.Date,
                            attendance.PassType
                        };

            var reportData = query.ToList();

            List<AttendanceReportItem> reportItems = new List<AttendanceReportItem>();

            foreach (var item in reportData)
            {
                string passType = item.PassType ? "УП" : "Н";
                AttendanceReportItem reportItem = new AttendanceReportItem
                {
                    SubjectName = item.SubjectName,
                    Date = item.Date,
                    PassType = passType
                };
                reportItems.Add(reportItem);
            }

            myDataGrid.ItemsSource = reportItems;
        }

        private int GetStudentId()
        {
            int currentUser = (int)Application.Current.Properties["UserId"];

            using (var dbContext = new BDEntities())
            {
                var student = dbContext.Students.FirstOrDefault(s => s.UserId == currentUser);
                if (student != null)
                {
                    return student.StudentId;
                }
            }

            return -1; // Если не удалось получить идентификатор студента, возвращаем значение по умолчанию
        }

        // Класс модели данных для отчета
        private class AttendanceReportItem
        {
            public string SubjectName { get; set; }
            public DateTime Date { get; set; }
            public string PassType { get; set; }
        }
    }
}
