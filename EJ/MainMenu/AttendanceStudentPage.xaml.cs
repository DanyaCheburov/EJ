using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
            SetCurrentMonthDates();
        }

        private void SetCurrentMonthDates()
        {
            // Установка начала текущего месяца
            DateTime startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            StartOfPeriod.SelectedDate = startOfMonth;

            // Установка конца текущего месяца
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            EndOfPeriod.SelectedDate = endOfMonth;
        }

        private void GetGroupName()
        {
            int currentUser = (int)Application.Current.Properties["UserId"];

            using (var dbContext = new BDEntities())
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

            // Создание списка уникальных дат и сортировка их по возрастанию
            var uniqueDates = reportData.Select(item => item.Date.Date).Distinct().OrderBy(date => date).ToList();

            // Создание списка объектов AttendanceReportItem
            List<AttendanceReportItem> reportItems = new List<AttendanceReportItem>();

            foreach (var item in reportData)
            {
                string passType = item.PassType ? "УП" : "Н";

                // Поиск соответствующего объекта AttendanceReportItem для предмета
                AttendanceReportItem reportItem = reportItems.FirstOrDefault(r => r.SubjectName == item.SubjectName);

                if (reportItem == null)
                {
                    reportItem = new AttendanceReportItem
                    {
                        SubjectName = item.SubjectName
                    };
                    reportItems.Add(reportItem);
                }

                // Заполнение данных о пропусках для каждой даты
                if (!reportItem.DateData.ContainsKey(item.Date.Date))
                {
                    reportItem.DateData.Add(item.Date.Date, passType);

                    if (passType == "УП")
                    {
                        reportItem.UPCount += 2;
                    }
                    else if (passType == "Н")
                    {
                        reportItem.NCount += 2;
                    }
                }
            }

            // Очистка существующих столбцов в DataGrid
            myDataGrid.Columns.Clear();

            // Добавление столбца для предметов в DataGrid
            DataGridTextColumn subjectColumn = new DataGridTextColumn();
            subjectColumn.Header = "Предмет";
            subjectColumn.Binding = new Binding("SubjectName");
            subjectColumn.IsReadOnly = true;
            myDataGrid.Columns.Add(subjectColumn);

            // Добавление столбцов для дат в DataGrid в отсортированном порядке
            foreach (var date in uniqueDates)
            {
                DataGridTextColumn dateColumn = new DataGridTextColumn();
                dateColumn.Header = date.ToString("dd.MM.yy");
                dateColumn.Binding = new Binding(string.Format("DateData[{0:yyyy-MM-dd}]", date.Date));
                dateColumn.IsReadOnly = true;
                myDataGrid.Columns.Add(dateColumn);
            }

            // Добавление столбца "Отсутствие (считает всего УП) по уважительной причине"
            DataGridTextColumn upCountColumn = new DataGridTextColumn();
            upCountColumn.Header = "Отсутствие\nпо уважительной\nпричине";
            upCountColumn.Binding = new Binding("UPCount");
            upCountColumn.IsReadOnly = true;
            myDataGrid.Columns.Add(upCountColumn);

            // Добавление столбца "Отсутствие (считает всего Н) по неуважительной причине"
            DataGridTextColumn nCountColumn = new DataGridTextColumn();
            nCountColumn.Header = "Отсутствие\nпо неуважительной\nпричине";
            nCountColumn.Binding = new Binding("NCount");
            nCountColumn.IsReadOnly = true;
            myDataGrid.Columns.Add(nCountColumn);

            // Обновление данных в DataGrid
            myDataGrid.ItemsSource = reportItems;
            myDataGrid.Visibility = Visibility.Visible;
        }

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
    }
}
