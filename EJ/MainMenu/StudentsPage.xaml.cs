using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private void SaveChangesButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            using (var db = new BDEntities())
            {
                foreach (var student in Students)
                {
                    db.Entry(student).State = System.Data.Entity.EntityState.Modified;
                    db.Entry(student.Users).State = System.Data.Entity.EntityState.Modified;
                }

                db.SaveChanges();
            }
        }

        private void AddStudentGroup_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}