using EJ.AttendanceManagement;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

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
            LoadTeachers();
            AdminRole();
        }
        private void AdminRole()
        {
            // Получение информации о текущем пользователе из БД
            int currentUser = (int)Application.Current.Properties["UserId"];

            // Проверка, является ли текущий пользователь администратором
            bool isAdmin = IsUserAdmin(currentUser);

            // Установка видимости кнопки
            if (isAdmin)
            {
                AddTeacher.Visibility = Visibility.Visible;
                DeleteButton.Visibility = Visibility.Visible;
            }
            else
            {
                AddTeacher.Visibility = Visibility.Collapsed;
                DeleteButton.Visibility = Visibility.Collapsed;
            }

            // Метод для проверки, является ли пользователь администратором
            bool IsUserAdmin(int userId)
            {
                using (var context = new BDEntities())
                {
                    // Проверка наличия пользователя с заданным UserId в таблице Administrators
                    isAdmin = context.Administrators.Any(a => a.UserId == userId);
                    return isAdmin;
                }
            }
        }
        private void LoadTeachers()
        {
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

        private void AddTeacher_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddTeacher();
            window.ShowDialog();
            UpdateTeachers();
        }
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTeacher = teacherDataGrid.SelectedItem as Teachers;

            if (selectedTeacher != null)
            {
                using (var dbContext = new BDEntities())
                {
                    var teacherToDelete = dbContext.Teachers.Find(selectedTeacher.TeacherId);
                    if (teacherToDelete != null)
                    {
                        dbContext.Teachers.Remove(teacherToDelete);
                        dbContext.SaveChanges();
                        Teachers.Remove(selectedTeacher);
                    }
                }
            }
        }
    }
}
