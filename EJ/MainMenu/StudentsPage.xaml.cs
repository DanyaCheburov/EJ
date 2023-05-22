using EJ.AttendanceManagement;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EJ.MainMenu
{
    public partial class StudentsPage : Page
    {
        public ObservableCollection<Students> Students { get; set; }

        public StudentsPage()
        {
            InitializeComponent();
            LoadStudents();
        }
        private void LoadStudents()
        {

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
            var selectedStudent = studentDataGrid.SelectedItem as Students;

            if (selectedStudent != null)
            {
                using (var dbContext = new BDEntities())
                {
                    var studentToDelete = dbContext.Students.Find(selectedStudent.StudentId);
                    if (studentToDelete != null)
                    {
                        dbContext.Students.Remove(studentToDelete);
                        dbContext.SaveChanges();
                        Students.Remove(selectedStudent);
                    }
                }
            }
        }
    }
}