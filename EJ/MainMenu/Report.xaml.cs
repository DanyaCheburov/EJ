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
        public Report()
        {
            InitializeComponent();

            ComboGroup.ItemsSource = _context.Groups.ToList();
            ComboCharTypes.ItemsSource = Enum.GetValues(typeof(SeriesChartType));
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (ComboGroup.SelectedItem is Groups currentGroup &&
                ComboCharTypes.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartPayments.Series.FirstOrDefault();
                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();

                var _connection = (@"Data Source=localhost\SQLEXPRESS;Initial Catalog=BD;Integrated Security=True");
                // Выбираем студентов для определенной группы и отображаем их на графике
                string query = "SELECT u.Name, COUNT(*) as Absences " +
                               "FROM Users AS u " +
                               "JOIN Students AS s ON s.UserId=u.Id " +
                               "JOIN Groups AS g ON g.GroupId=s.GroupId " +
                               "JOIN Attendance AS a ON a.StudentId=s.Id " +
                               "WHERE g.GroupName = @GroupName AND a.PassType = 0 " +
                               "GROUP BY u.Name";
                SqlConnection connection = new SqlConnection(_connection);
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@GroupName", currentGroup.GroupName);

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


        private void Print_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
