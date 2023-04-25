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

        private void AddTeacherGroup_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddTeacher();
            window.ShowDialog();
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new BDEntities())
            {
                foreach (var student in Teachers)
                {
                    db.Entry(student).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(student.Users).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges();
            }
        }
    }
}
