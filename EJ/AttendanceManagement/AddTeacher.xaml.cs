using System.Linq;
using System.Windows;
using System.Windows.Input;

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
            var selectedPosition = Position.Text;
            var existingTeacher = db.Teachers.FirstOrDefault(t => t.UserId == selectedUser.UserId);

            if (existingTeacher != null)
            {
                MessageBox.Show("Преподаватель уже существует.");
            }
            else
            {
                var newTeacher = new Teachers { UserId = selectedUser.UserId, Department = "abc", Position = selectedPosition};
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
