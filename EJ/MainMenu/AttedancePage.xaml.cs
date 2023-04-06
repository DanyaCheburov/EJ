using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using EJ.Properties;

namespace EJ.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для AttedancePage.xaml
    /// </summary>
    public partial class AttedancePage : Page
    {
        public ObservableCollection<Students> Students { get; set; }
        
        private int DaysInMonth;
        private DateTime dtStart;
        private DateTime dtEnd;

        public class EmployeeAttendance
        {
            public string StudentId { get; set; }
            public string Day1 { get; set; }
            public string Day2 { get; set; }
            public string Day3 { get; set; }
            public string Day4 { get; set; }
            public string Day5 { get; set; }
            public string Day6 { get; set; }
            public string Day7 { get; set; }
            public string Day8 { get; set; }
            public string Day9 { get; set; }
            public string Day10 { get; set; }
            public string Day11 { get; set; }
            public string Day12 { get; set; }
            public string Day13 { get; set; }
            public string Day14 { get; set; }
            public string Day15 { get; set; }
            public string Day16 { get; set; }
            public string Day17 { get; set; }
            public string Day18 { get; set; }
            public string Day19 { get; set; }
            public string Day20 { get; set; }
            public string Day21 { get; set; }
            public string Day22 { get; set; }
            public string Day23 { get; set; }
            public string Day24 { get; set; }
            public string Day25 { get; set; }
            public string Day26 { get; set; }
            public string Day27 { get; set; }
            public string Day28 { get; set; }
            public string Day29 { get; set; }
            public string Day30 { get; set; }
            public string Day31 { get; set; }
        }

        public AttedancePage()
        {
            InitializeComponent();
            CreateTable();
            LoadGrid();
            ComboSubject.ItemsSource = BDEntities.GetContext().Subjects.ToList();
            ComboGroup.ItemsSource = BDEntities.GetContext().Groups.ToList();
           

           using (var db = new BDEntities())
            {
                var students = db.Students.Include("Users").ToList();
                Students = new ObservableCollection<Students>(students);
            }

            DataContext = this;

            //Автоматическое добавление даты
            comboBox.Items.Add(2019);

            // Add all years between 2019 and the current year to the combo box
            int currentYear = DateTime.Now.Year;
            for (int year = 2019; year <= currentYear; year++)
            {
                if (!comboBox.Items.Contains(year))
                {
                    comboBox.Items.Add(year);
                }
            }
            comboBox.SelectedItem = currentYear;
        }
        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
           int currentMonthIndex = DateTime.Now.Month - 1;
            ComboBox ComboMonth = sender as ComboBox;
            ComboMonth.SelectedIndex = currentMonthIndex;
        }

        private void add_attedance_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddAttedance();
            window.ShowDialog();
        }

        private void ComboGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (var db = new BDEntities())
            {
                // Получаем выбранную группу
                var selectedGroup = ComboGroup.SelectedItem as Groups;

                // Фильтруем список студентов по выбранной группе
                var filteredStudents = db.Students
                    .Include("Users")
                    .Where(s => s.GroupId == selectedGroup.GroupId)
                    .ToList();

                // Обновляем коллекцию студентов с отфильтрованными данными
                Students.Clear();
                foreach (var student in filteredStudents)
                {
                    Students.Add(student);
                }
            }

            // Обновляем столбцы таблицы для отображения имени студента для выбранной группы
            DataGridTextColumn nameColumn = myDataGrid.Columns[0] as DataGridTextColumn;
            nameColumn.Binding = new Binding("StudentId");

            // Добавляем столбцы для каждого месяца для отслеживания посещаемости
            ComboBoxItem selectedMonthItem = ComboMonth.SelectedItem as ComboBoxItem;
            string selectedMonth = selectedMonthItem.Content.ToString();
            int selectedYear = (int)comboBox.SelectedItem;
        }


        private void CreateTable()
        {
            var dt = DateTime.Today;

            DaysInMonth = DateTime.DaysInMonth(dt.Year, dt.Month);
            dtStart = new DateTime(dt.Year, dt.Month, 1);
            dtEnd = dtStart.AddDays(DaysInMonth - 1);

            for (int i = 1; i <= DaysInMonth; i++)
            {
                myDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = i.ToString(),
                    Binding = new Binding($"Day{i}")
                });
            }
        }

        private void LoadGrid()
        {
            using (var db = new BDEntities())
            {
                var query = from a in db.Attendance
                            where a.Date >= dtStart && a.Date <= dtEnd
                            orderby a.StudentId, a.Date
                            select new { a.StudentId, a.Date };

                var rstEdata = query.ToList();

                var employeeAttendanceList = new List<EmployeeAttendance>();
                var employeeAttendance = new EmployeeAttendance();
                var lastEmpID = -1;

                foreach (var row in rstEdata)
                {
                    var empID = row.StudentId;
                    var day = row.Date.Day.ToString();

                    if (empID != lastEmpID)
                    {
                        if (lastEmpID != -1)
                        {
                            employeeAttendanceList.Add(employeeAttendance);
                        }
                        employeeAttendance = new EmployeeAttendance { StudentId = empID.ToString() };
                        lastEmpID = empID;
                    }

                    var propertyDescriptor = TypeDescriptor.GetProperties(typeof(EmployeeAttendance))[$"Day{day}"];
                    if (propertyDescriptor != null)
                    {
                        propertyDescriptor.SetValue(employeeAttendance, "H");
                    }
                }

                employeeAttendanceList.Add(employeeAttendance);

                myDataGrid.ItemsSource = employeeAttendanceList;
            }
        }

        private void reflesh_attedance_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Refresh();
        }
    }
}
