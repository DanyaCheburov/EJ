using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;

namespace EJ.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для AttedancePage.xaml
    /// </summary>
    public partial class AttedancePage : Page
    {
        public ObservableCollection<Students> Students { get; set; }

        public AttedancePage()
        {
            InitializeComponent();
            CreateTable();
            ComboSubject.ItemsSource = BDEntities.GetContext().Subjects.ToList();
            ComboGroup.ItemsSource = BDEntities.GetContext().Groups.ToList();

            using (var db = new BDEntities())
            {
                var students = db.Students.Include("Users").ToList();
                Students = new ObservableCollection<Students>(students);
            }

            DataContext = this;

            //Автоматическое добавление даты
            СomboYear.Items.Add(2019);

            // Add all years between 2019 and the current year to the combo box
            int currentYear = DateTime.Now.Year;
            for (int year = 2019; year <= currentYear; year++)
            {
                if (!СomboYear.Items.Contains(year))
                {
                    СomboYear.Items.Add(year);
                }
            }
            СomboYear.SelectedItem = currentYear;
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

        private void LoadGrid()
        {
            string groupName = null;
            if (ComboGroup.SelectedItem != null)
            {
                groupName = ((Groups)ComboGroup.SelectedItem).GroupName;
            }
            if (string.IsNullOrEmpty(groupName))
            {
                return;
            }

            int year = (int)СomboYear.SelectedItem;
            int month = ComboMonth.SelectedIndex + 1;
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            using (var db = new BDEntities())
            {
                var query = from s in db.Students
                            join g in db.Groups on s.GroupId equals g.GroupId
                            join u in db.Users on s.UserId equals u.Id
                            join a in db.Attendance.Where(a => a.Date >= startDate && a.Date <= endDate)
                                on s.Id equals a.StudentId into aGroup
                            from a in aGroup.DefaultIfEmpty()
                            where g.GroupName == groupName
                            orderby s.Id
                            select new { u.Name, StudentId = s.Id, Date = (a != null ? a.Date : default(DateTime?)), HasAbsence = (a != null) };

                var rstEdata = query.ToList();

                var employeeAttendanceList = new List<EmployeeAttendance>();
                var employeeAttendance = new EmployeeAttendance();
                var lastEmpID = -1;

                foreach (var row in rstEdata)
                {
                    var empID = row.StudentId;
                    var day = row.Date?.Day.ToString() ?? "";

                    if (empID != lastEmpID)
                    {
                        if (lastEmpID != -1)
                        {
                            employeeAttendanceList.Add(employeeAttendance);
                        }
                        employeeAttendance = new EmployeeAttendance { Name = row.Name }; // заменяем StudentId на Name
                        lastEmpID = empID;
                    }

                    var propertyDescriptor = TypeDescriptor.GetProperties(typeof(EmployeeAttendance))[$"Day{day}"];
                    propertyDescriptor?.SetValue(employeeAttendance, "H");
                }

                employeeAttendanceList.Add(employeeAttendance);

                myDataGrid.ItemsSource = employeeAttendanceList;
            }
        }

        private void ComboGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGrid();
        }


        private void CreateTable()
        {
            for (int i = 1; i <= 31; i++)
            {
                myDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = i.ToString(),
                    Binding = new Binding($"Day{i}")
                });
            }
        }

        private void reflesh_attedance_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Refresh();
        }

        private void ComboMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGrid();
        }

        private void ComboYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGrid();
        }
    }
}