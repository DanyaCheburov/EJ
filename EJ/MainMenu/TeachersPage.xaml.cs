using EJ.AttendanceManagement;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EJ.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для TeachersPage.xaml
    /// </summary>
    public partial class TeachersPage : Page
    {
        public ObservableCollection<Teachers> Teachers { get; set; }
        public TeachersPage()
        {
            InitializeComponent();
            using (var db = new BDEntities())
            {
                var teachers = db.Teachers.Include("Users").ToList();
                Teachers = new ObservableCollection<Teachers>(teachers);
            }

            DataContext = this;
        }

        private void UpdateTeachers()
        {
            using (var db = new BDEntities())
            {
                var teachers = db.Teachers.Include("Users").ToList();
                Teachers.Clear();
                foreach (var teacher in teachers)
                {
                    Teachers.Add(teacher);
                }
            }
        }

        private void AddTeacherGroup_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddTeacher();
            window.ShowDialog();
            UpdateTeachers();
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTeacher = teacherDataGrid.SelectedItem as Teachers; // Получаем выбранного студента из DataGrid

            if (selectedTeacher != null)
            {
                // Выполняем удаление из базы данных
                using (var dbContext = new BDEntities())
                {
                    var teacherToDelete = dbContext.Teachers.Find(selectedTeacher.TeacherId);
                    if (teacherToDelete != null)
                    {
                        dbContext.Teachers.Remove(teacherToDelete);
                        dbContext.SaveChanges();
                        // Удаляем студента из коллекции Students, чтобы обновить DataGrid
                        Teachers.Remove(selectedTeacher);
                    }
                }
            }
        }
    }
}
