using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using EJ.MainMenu;

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
            using (var db = new BDEntities())
            {
                var query = from a in db.Attendance
                            join s in db.Students on a.StudentId equals s.Id
                            join g in db.Groups on s.GroupId equals g.GroupId
                            join u in db.Users on s.UserId equals u.Id
                            where a.Date >= dtStart && a.Date <= dtEnd && g.GroupName == groupName
                            orderby s.Id, a.Date
                            select new { u.Name, a.Date, a.StudentId };

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
                        employeeAttendance = new EmployeeAttendance { Name = row.Name }; // заменяем StudentId на Name
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

        private void ComboGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            LoadGrid();
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


        private void reflesh_attedance_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Refresh();
        }
    }
}
