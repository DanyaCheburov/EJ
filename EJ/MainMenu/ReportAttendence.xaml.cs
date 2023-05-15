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

namespace EJ.MainMenu
{
    public partial class Report : Window
    {
        private BDEntities _context = new BDEntities();
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
            ComboGroup.SelectedItem = _context.Groups.FirstOrDefault(g => g.GroupName == SelectedGroup); // установить выбранное значение ComboGroup
            SelectedSubject = selectedSubject;
            ComboSubject.ItemsSource = _context.Subjects.ToList();
            ComboSubject.SelectedItem = _context.Subjects.FirstOrDefault(s => s.SubjectName == SelectedSubject);                
            
            ComboYear.ItemsSource = Enumerable.Range(2019, DateTime.Now.Year - 2018); // используем метод Enumerable.Range для заполнения ComboBox годами с 2019 до текущего года
            ComboYear.SelectedItem = selectedYear;

            ComboMonth.ItemsSource = CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Take(12).ToList();
            ComboMonth.SelectedItem = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(selectedMonth+1);
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
            if (ComboGroup.SelectedItem is Groups currentGroup &&
                 ComboSubject.SelectedItem is Subjects currentSubject &&
                 ComboMonth.SelectedItem is string selectedMonthName)
            {
                ChartPayments.Series.Clear();
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

                ChartPayments.Series.Add(currentSeries);

                var _connection = (@"Data Source=localhost\SQLEXPRESS;Initial Catalog=BD;Integrated Security=True");
                string lessonsQuery = "SELECT COUNT(*) as Lessons FROM Lessons_by_subject WHERE Subject_Id = @SubjectId";
                SqlConnection lessonsConnection = new SqlConnection(_connection);
                SqlCommand lessonsCommand = new SqlCommand(lessonsQuery, lessonsConnection);
                lessonsCommand.Parameters.AddWithValue("@SubjectId", currentSubject.SubjectId);
                lessonsConnection.Open();
                int numberOfLessons = (int)lessonsCommand.ExecuteScalar();
                lessonsConnection.Close();

                currentSeries["PointHeight"] = $"{numberOfLessons / 100.0:P0}";
                currentSeries["HeightPercent"] = "100";

                int selectedMonth = Array.IndexOf(CultureInfo.CurrentCulture.DateTimeFormat.MonthNames, selectedMonthName);

                // Выбираем студентов для определенной группы и отображаем их на графике
                string query = "SELECT u.UserName, COUNT(*) as Absences, l.Nubmer_of_lessons AS Lessons " +
                               "FROM Users AS u " +
                               "JOIN Students AS s ON s.UserId=u.UserId " +
                               "JOIN Groups AS g ON g.GroupId=s.GroupId " +
                               "JOIN Attendance AS a ON a.StudentId=s.StudentId " +
                               "JOIN Subjects AS s1 ON s1.SubjectId = a.SubjectId " +
                               "JOIN Lessons_by_subject AS l ON s1.SubjectId = l.Subject_Id " +
                               "WHERE g.GroupName = @GroupName  AND s1.SubjectName=@Name AND a.PassType = 0 AND MONTH(a.Date) = @Month AND YEAR(a.Date) = @Year " +
                               "GROUP BY u.UserName, l.Nubmer_of_lessons " +
                               "HAVING COUNT(a.PassType) > 0";
                SqlConnection connection = new SqlConnection(_connection);
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@GroupName", currentGroup.GroupName);
                command.Parameters.AddWithValue("@Name", currentSubject.SubjectName);
                command.Parameters.AddWithValue("@Month", selectedMonth + 1);
                command.Parameters.AddWithValue("@Year", SelectedYear);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    double maxNumberOfLessons = 0; // используйте double для максимального значения
                    double height = 0; // определите переменную для высоты столбцов

                    while (reader.Read())
                    {
                        string studentName = reader.GetString(0);
                        int absences = reader.GetInt32(1);
                        numberOfLessons = reader.GetInt32(2);
                        numberOfLessons *= 2;
                        height = (double)absences*2; // вычисляем высоту столбца
                        maxNumberOfLessons = Math.Max(maxNumberOfLessons, numberOfLessons);

                        var dataPoint = new DataPoint();
                        dataPoint.SetValueY(height); // используйте переменную height для установки значения Y
                        dataPoint.AxisLabel = studentName;
                        double percentAbsent = (double)absences / (double)numberOfLessons; // вычисляем процент пропущенных занятий
                        dataPoint.Label = $"{percentAbsent:P0}"; // устанавливаем метку, содержащую процентное соотношение пропущенных занятий к общему количеству занятий
                        currentSeries.Points.Add(dataPoint);
                    }
                    reader.Close();
                    ChartPayments.ChartAreas[0].AxisY.Maximum = Math.Ceiling(maxNumberOfLessons); // устанавливаем максимальное значение для оси Y
                    ChartPayments.ChartAreas[0].AxisY.Minimum = 0; // устанавливаем минимальное значение для оси Y

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
        }
    }
}