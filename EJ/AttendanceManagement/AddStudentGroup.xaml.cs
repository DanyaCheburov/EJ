using System.Linq;
using System.Windows;
using System.Windows.Input;

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

            var existingStudent = db.Students.FirstOrDefault(s => s.UserId == selectedUser.UserId);

            if (existingStudent != null)
            {
                existingStudent.GroupId = selectedGroup.GroupId;
                db.SaveChanges();
                MessageBox.Show("Группа для студента обновлена.");
            }
            else
            {
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