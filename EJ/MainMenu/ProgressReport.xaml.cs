using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Xml.Linq;

namespace EJ.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для ProgressReport.xaml
    /// </summary>
    public partial class ProgressReport : Window
    {
        private BDEntities _context = new BDEntities();
        public string SelectedGroup { get; set; }
        public string SelectedSubject { get; set; }
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }
        
        public ProgressReport(string selectedGroup, string selectedSubject, int selectedYear, int selectedMonth)
        {
            InitializeComponent();
            
            SelectedYear = selectedYear;
            SelectedGroup = selectedGroup;
            ComboGroup.ItemsSource = _context.Groups.ToList();
            ComboGroup.SelectedItem = _context.Groups.FirstOrDefault(g => g.GroupName == SelectedGroup); // установить выбранное значение ComboGroup
            SelectedSubject = selectedSubject;
            ComboSubject.ItemsSource = _context.Subjects.ToList();
            ComboSubject.SelectedItem = _context.Subjects.FirstOrDefault(s => s.SubjectName == SelectedSubject);

            ComboYear.ItemsSource = Enumerable.Range(2019, DateTime.Now.Year - 2018); // используем метод Enumerable.Range для заполнения ComboBox годами с 2019 до текущего года
            ComboYear.SelectedItem = selectedYear;

            ComboMonth.ItemsSource = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12).ToList();
            ComboMonth.SelectedItem = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(selectedMonth + 1);

        }
        private IEnumerable<Students> Students
        {
            get
            {
                if (ComboGroup.SelectedItem != null)
                {
                    var selectedGroup = ComboGroup.SelectedItem as Groups;
                    return BDEntities.GetContext().Students.Where(s => s.GroupId == selectedGroup.GroupId).ToList();
                }
                return null;
            }
        }
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void UpdateChart(object sender, RoutedEventArgs e)
        {
            ComboStudents.ItemsSource = Students;
            if (ComboGroup.SelectedItem is Groups currentGroup &&
                 ComboSubject.SelectedItem is Subjects currentSubject &&
                 ComboMonth.SelectedItem is string selectedMonthName &&
                 ComboStudents.SelectedItem is Students currentStudent) 
            {
                ChartsEtimates.Series.Clear();
                Series currentSeries = new Series
                {
                    Name = "",
                    ChartType = SeriesChartType.Pie
                };
                var selectedGroup = ComboGroup.SelectedItem as Groups;
                var selectedSubject = ComboSubject.SelectedItem as Subjects;
                var selectedMonth = ComboMonth.SelectedIndex + 1;
                var selectedYear = Convert.ToInt32(ComboYear.SelectedItem);

                  
                using (var db = new BDEntities())
                {
                    var query = from j in _context.Journal
                                  join sb in _context.Subjects on j.SubjectId equals sb.SubjectId
                                  join st in _context.Students on j.StudentId equals st.StudentId
                                  join u in _context.Users on st.UserId equals u.UserId
                                  join g in _context.Groups on st.GroupId equals g.GroupId
                                  where g.GroupName == selectedGroup.GroupName
                                      && sb.SubjectName == selectedSubject.SubjectName
                                      && j.Date.Month == selectedMonth
                                      && j.Date.Year == selectedYear
                                  select new
                                  {
                                      GroupName = g.GroupName,
                                      UserName = u.UserName,
                                      SubjectName = sb.SubjectName,
                                      Estimate = j.Estimate,
                                      Date = j.Date
                                  };
                    var grades = query.ToList();
                    Dictionary<string, int> gradesCount = new Dictionary<string, int>();
                    foreach (var grade in grades)
                    {
                        if (gradesCount.ContainsKey(grade.Estimate.ToString()))
                        {
                            gradesCount[grade.Estimate.ToString()]++;
                        }
                        else
                        {
                            gradesCount.Add(grade.Estimate.ToString(), 1);
                        }
                        
                    }

                    var gradeLabels = new List<string>();

                    foreach (var item in gradesCount)
                    {
                        double percentage = (double)item.Value / grades.Count * 100;
                        string label = $" {percentage:f2}%";
                        gradeLabels.Add(item.Key); // добавляем оценку в коллекцию
                        currentSeries.Points.AddXY(label, item.Value);
                    }

                    ChartsEtimates.Series.Clear();
                    ChartsEtimates.Series.Add(currentSeries);

                    // устанавливаем метки оценок в легенде
                    for (int i = 0; i < gradeLabels.Count; i++)
                    {
                        ChartsEtimates.Series[0].Points[i].LegendText = gradeLabels[i];
                    }
                }

            }

        }
    }
}
