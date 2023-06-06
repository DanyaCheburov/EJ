using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;

namespace EJ.MainMenu
{
    public partial class Report : Window
    {
        private readonly BDEntities _context = new BDEntities();
        public string SelectedGroup { get; set; }
        public string SelectedSubject { get; set; }
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }
        public Report(string selectedGroup, string selectedSubject, int selectedYear, int selectedMonth)
        {
            InitializeComponent();
            SelectedYear = selectedYear;
            SelectedGroup = selectedGroup;
            ComboGroup.ItemsSource = _context.Groups.ToList();
            ComboGroup.SelectedItem = _context.Groups.FirstOrDefault(g => g.GroupName == SelectedGroup);
            SelectedSubject = selectedSubject;
            ComboSubject.ItemsSource = _context.Subjects.ToList();
            ComboSubject.SelectedItem = _context.Subjects.FirstOrDefault(s => s.SubjectName == SelectedSubject);

            ComboYear.ItemsSource = Enumerable.Range(2019, DateTime.Now.Year - 2018);
            ComboYear.SelectedItem = selectedYear;

            ComboMonth.ItemsSource = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12).ToList();
            ComboMonth.SelectedItem = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(selectedMonth + 1);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
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

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            int numberOfLessons = 0;
            if (ComboGroup.SelectedItem is Groups currentGroup &&
                 ComboSubject.SelectedItem is Subjects currentSubject &&
                 ComboMonth.SelectedItem is string selectedMonthName)
            {
                ChartAttendences.Series.Clear();
                Series currentSeries = new Series
                {
                    Name = "Пропуски по неуважительной причине",
                    ChartType = SeriesChartType.Column,
                    YValueType = ChartValueType.Int32,
                    YValuesPerPoint = 1
                };
                currentSeries["PixelPointWidth"] = "30";
                currentSeries["PointWidth"] = "1";
                currentSeries.CustomProperties = "PixelPointWidth=40, MaxPixelPointWidth=100";

                ChartAttendences.Series.Add(currentSeries);

                using (var context = new BDEntities())
                {
                    var numberOfLessonsQuery = context.Lessons_by_subject
                        .Where(l => l.SubjectId == currentSubject.SubjectId)
                        .Select(l => new { l.SubjectId, l.NubmerOfLessons })
                        .ToList();

                    numberOfLessons = numberOfLessonsQuery.Sum(l => l.NubmerOfLessons);

                    currentSeries["PointHeight"] = $"{numberOfLessons / 100.0:P0}";
                    currentSeries["HeightPercent"] = "100";

                    int selectedMonth = Array.IndexOf(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames, selectedMonthName);

                    var query = from u in context.Users
                                join s in context.Students on u.UserId equals s.UserId
                                join g in context.Groups on s.GroupId equals g.GroupId
                                join a in context.Attendance on s.StudentId equals a.StudentId
                                join s1 in context.Subjects on a.SubjectId equals s1.SubjectId
                                join l in context.Lessons_by_subject on s1.SubjectId equals l.SubjectId
                                where g.GroupName == currentGroup.GroupName &&
                                      s1.SubjectName == currentSubject.SubjectName &&
                                      a.PassType == false &&
                                      a.Date.Month == selectedMonth + 1 &&
                                      a.Date.Year == SelectedYear
                                group new { u.UserName, l.NubmerOfLessons } by new { u.UserName, l.NubmerOfLessons } into grp
                                where grp.Count() > 0
                                select new
                                {
                                    grp.Key.UserName,
                                    Absences = grp.Count(),
                                    Lessons = grp.Key.NubmerOfLessons
                                };

                    try
                    {
                        double maxNumberOfLessons = 0;
                        double height = 0;
                        foreach (var result in query)
                        {
                            string studentName = result.UserName;
                            int absences = result.Absences;
                            height = (double)absences * 2;
                            maxNumberOfLessons = Math.Max(maxNumberOfLessons, numberOfLessons);

                            var dataPoint = new DataPoint();
                            dataPoint.SetValueY(height);
                            dataPoint.AxisLabel = studentName;
                            double percentAbsent = (double)absences / (double)numberOfLessons;
                            dataPoint.Label = $"{percentAbsent:P0}"; 
                            currentSeries.Points.Add(dataPoint);
                        }

                        ChartAttendences.ChartAreas[0].AxisY.Maximum = Math.Ceiling(maxNumberOfLessons * 2); 
                        ChartAttendences.ChartAreas[0].AxisY.Minimum = 0;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
    }
}