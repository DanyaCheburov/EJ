using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EJ.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для JournalPage.xaml
    /// </summary>
    public partial class JournalPage : Page
    {
        public ObservableCollection<Students> Students { get; set; }

        public JournalPage()
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

            int subjectId = 0;
            if (ComboSubject.SelectedItem != null)
            {
                subjectId = ((Subjects)ComboSubject.SelectedItem).SubjectId;
            }

            using (var db = new BDEntities())
            {
                var query = from s in db.Students
                            join g in db.Groups on s.GroupId equals g.GroupId
                            join u in db.Users on s.UserId equals u.Id
                            join j in db.Journal.Where(j => j.Date >= startDate && j.Date <= endDate && j.SubjectId == subjectId)
                                on s.Id equals j.StudentId into aGroup
                            from a in aGroup.DefaultIfEmpty()
                            join sub in db.Subjects on a.SubjectId equals sub.SubjectId into subGroup
                            from sub in subGroup.DefaultIfEmpty()
                            where g.GroupName == groupName
                            orderby s.Id
                            select new { u.Name, StudentId = s.Id, Date = (a != null ? a.Date : default(DateTime?)), HasAbsence = (a != null), SubjectName = (sub != null ? sub.Name : ""), Score = (a != null ? a.Estimate : default(int)) };

                var rstEdata = query.ToList();

                var employeeJournalList = new List<EmployeeJournal>();
                var employeeJournal = new EmployeeJournal();
                var lastEmpID = -1;

                foreach (var row in rstEdata)
                {

                    var empID = row.StudentId;
                    var day = row.Date?.Day.ToString() ?? "";

                    if (empID != lastEmpID)
                    {
                        if (lastEmpID != -1)
                        {
                            employeeJournalList.Add(employeeJournal);
                        }
                        employeeJournal = new EmployeeJournal { Name = row.Name }; // заменяем StudentId на Name
                        lastEmpID = empID;
                    }
                    var score = row.Score.ToString() ?? "";
                    var propertyDescriptor = TypeDescriptor.GetProperties(typeof(EmployeeJournal))[$"Day{day}"];
                    propertyDescriptor?.SetValue(employeeJournal, score);

                }

                employeeJournalList.Add(employeeJournal);

                myDataGrid.ItemsSource = employeeJournalList;

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
                    Binding = new Binding($"Day{i}"),
                    IsReadOnly = true
                });
            }


        }
        private void ComboMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGrid();
        }

        private void ComboYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGrid();
        }

        private void ComboSubject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGrid();
        }

        private void Add_estimate_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEstimate();
            window.ShowDialog();
            LoadGrid();
        }
    }
}