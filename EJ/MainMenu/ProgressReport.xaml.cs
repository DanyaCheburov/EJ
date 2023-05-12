using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
            ComboStudents.ItemsSource=_context.Users.ToList();
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

        }
    }
}
