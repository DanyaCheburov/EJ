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
    /// Логика взаимодействия для AddStudentGroup.xaml
    /// </summary>
    public partial class AddStudentGroup : Window
    {
        public static BDEntities db = new BDEntities();
        public AddStudentGroup()
        {
            InitializeComponent();
            ComboGroup.ItemsSource = BDEntities.GetContext().Groups.ToList();
            ComboUsers.ItemsSource = BDEntities.GetContext().Users.ToList();
        }
        private void Add_student_Click(object sender, RoutedEventArgs e)
        {
            var selectedGroup = (Groups)ComboGroup.SelectedItem;
            var selectedUser = (Users)ComboUsers.SelectedItem;

            // Проверяем, есть ли у пользователя уже группа
            var existingStudent = db.Students.FirstOrDefault(s => s.UserId == selectedUser.UserId);

            if (existingStudent != null)
            {
                // Если группа есть, то обновляем ее значение
                existingStudent.GroupId = selectedGroup.GroupId;
                db.SaveChanges();
                MessageBox.Show("Группа для студента обновлена.");
            }
            else
            {
                // Иначе создаем новую запись для студента
                var newStudent = new Students { UserId = selectedUser.UserId, GroupId = selectedGroup.GroupId };
                db.Students.Add(newStudent);
                db.SaveChanges();
                MessageBox.Show("Студент успешно добавлен в группу.");
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