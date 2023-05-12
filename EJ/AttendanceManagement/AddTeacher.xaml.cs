using System;
using System.Collections.Generic;
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

namespace EJ.AttendanceManagement
{
    /// <summary>
    /// Логика взаимодействия для AddTeacher.xaml
    /// </summary>
    public partial class AddTeacher : Window
    {
        public static BDEntities db = new BDEntities();
        public AddTeacher()
        {
            InitializeComponent();
            ComboUsers.ItemsSource = BDEntities.GetContext().Users.ToList();
        }
        private void Add_Teacher_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = (Users)ComboUsers.SelectedItem;

            // Проверяем, есть ли уже такой преподаватель
            var existingTeacher = db.Teachers.FirstOrDefault(t => t.UserId == selectedUser.UserId);

            if (existingTeacher != null)
            {
                MessageBox.Show("Преподаватель уже существует.");
            }
            else
            {
                // Иначе создаем новую запись для преподавателя
                var newTeacher = new Teachers { UserId = selectedUser.UserId };
                db.Teachers.Add(newTeacher);
                db.SaveChanges();
                MessageBox.Show("Преподаватель успешно добавлен.");
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
