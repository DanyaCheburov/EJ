using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EJ.Properties;

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

        private void ComboGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (var db = new BDEntities())
            {
                // Получаем выбранную группу
                var selectedGroup = ComboGroup.SelectedItem as Groups;

                // Фильтруем список студентов по выбранной группе
                var filteredStudents = db.Students
                    .Include("Users")
                    .Where(s => s.GroupId == selectedGroup.GroupId)
                    .ToList();

                // Обновляем коллекцию студентов с отфильтрованными данными
                Students.Clear();
                foreach (var student in filteredStudents)
                {
                    Students.Add(student);
                }
            }

            // Обновляем столбцы таблицы для отображения имени студента для выбранной группы
            DataGridTextColumn nameColumn = DGridUser.Columns[0] as DataGridTextColumn;
            nameColumn.Binding = new Binding("Users.Name");

            // Добавляем столбцы для каждого месяца для отслеживания посещаемости
            ComboBoxItem selectedMonthItem = ComboMonth.SelectedItem as ComboBoxItem;
            string selectedMonth = selectedMonthItem.Content.ToString();
            int selectedYear = (int)comboBox.SelectedItem;
        }

    }
}
