using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EJ.MainMenu
{
    public partial class StudentsPage : Page
    {
        public ObservableCollection<Students> Students { get; set; }

        public StudentsPage()
        {
            InitializeComponent();
           
            using (var db = new BDEntities())
            {
                var students = db.Students.Include("Users").ToList();
                var groups = db.Students.Include("Groups").ToList();
                Students = new ObservableCollection<Students>(students);
            }

            DataContext = this;
        }
        private void UpdateStudents()
        {
            using (var db = new BDEntities())
            {
                var students = db.Students.Include("Users").ToList();
                var groups = db.Students.Include("Groups").ToList();
                Students.Clear();
                foreach (var student in students)
                {
                    Students.Add(student);
                }
            }
        }
        private void AddStudentGroup_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddStudentGroup();
            window.ShowDialog();
            UpdateStudents();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedStudent = studentDataGrid.SelectedItem as Students; // Получаем выбранного студента из DataGrid

            if (selectedStudent != null)
            {
                // Выполняем удаление из базы данных
                using (var dbContext = new BDEntities())
                {
                    var studentToDelete = dbContext.Students.Find(selectedStudent.StudentId);
                    if (studentToDelete != null)
                    {
                        dbContext.Students.Remove(studentToDelete);
                        dbContext.SaveChanges();
                        // Удаляем студента из коллекции Students, чтобы обновить DataGrid
                        Students.Remove(selectedStudent);
                    }
                }
            }
        }
    }
}