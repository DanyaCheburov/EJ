using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EJ.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для Report.xaml
    /// </summary>
    public partial class Report : Window
    {
        private BDEntities _context = new BDEntities();
        public Report()
        {
            InitializeComponent();
            ChartPayments.ChartAreas.Add(new ChartArea("Main"));

            var currentSeries = new Series("Payments")
            {
                IsValueShownAsLabel = true
            };
            ChartPayments.Series.Add(currentSeries);

            ComboUser.ItemsSource = _context.Users.ToList();
            ComboCharTypes.ItemsSource = Enum.GetValues(typeof(SeriesChartType));
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (ComboUser.SelectedItem is Users currentUser &&
                ComboCharTypes.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartPayments.Series.FirstOrDefault();
                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();

                var groupsList = _context.Groups.ToList();
                foreach (var group in groupsList)
                {
                    currentSeries.Points.AddXY(group.GroupName,
                        _context.Attendance.ToList().Where(a => a.StudentId == currentUser.Id
                        && a.SubjectId == group.GroupId && a.PassType == true).Count());
                }
            }
        }


        private void Print_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}