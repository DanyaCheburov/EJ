using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;

namespace EJ.MainMenu
{
    public partial class Report : Window
    {
        private BDEntities _context = new BDEntities();
        public string SelectedGroup { get; set; }
        public string SelectedSubject { get; set; }
        public Report(string selectedGroup, string selectedSubject)
        {

            InitializeComponent();
            SelectedGroup = selectedGroup;
            ComboGroup.ItemsSource = _context.Groups.ToList();
            ComboGroup.SelectedItem = _context.Groups.FirstOrDefault(g => g.GroupName == SelectedGroup); // установить выбранное значение ComboGroup
            SelectedSubject = selectedSubject;
            ComboSubject.ItemsSource = _context.Subjects.ToList();
            ComboSubject.SelectedItem = _context.Subjects.FirstOrDefault(s => s.Name == SelectedSubject);
        }


        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (ComboGroup.SelectedItem is Groups currentGroup &&
                ComboSubject.SelectedItem is Subjects currentSubject)
            {
                ChartPayments.Series.Clear();
                Series currentSeries = new Series
                {
                    Name = "Пропуски по неуважительной причине",
                    ChartType = SeriesChartType.Column
                };
                ChartPayments.Series.Add(currentSeries);

                var _connection = (@"Data Source=localhost\SQLEXPRESS1;Initial Catalog=BD;Integrated Security=True");
                // Выбираем студентов для определенной группы и отображаем их на графике
                string query = "SELECT u.Name, COUNT(*) as Absences " +
                               "FROM Users AS u " +
                               "JOIN Students AS s ON s.UserId=u.Id " +
                               "JOIN Groups AS g ON g.GroupId=s.GroupId " +
                               "JOIN Attendance AS a ON a.StudentId=s.Id " +
                               "JOIN Subjects AS s1 ON s1.SubjectId = a.SubjectId " +
                               "WHERE g.GroupName = @GroupName  AND s1.Name=@Name AND a.PassType = 0 " +
                               "GROUP BY u.Name";
                SqlConnection connection = new SqlConnection(_connection);
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@GroupName", currentGroup.GroupName);
                command.Parameters.AddWithValue("@Name", currentSubject.Name);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string studentName = reader.GetString(0);
                        double absences = Convert.ToDouble(reader.GetValue(1));
                        currentSeries.Points.AddXY(studentName, absences);
                    }
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