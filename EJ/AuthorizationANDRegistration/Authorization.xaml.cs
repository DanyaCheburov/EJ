using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace EJ.AuthorizationANDRegistration
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        public Authorization()
        {
            InitializeComponent();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = BDEntities.db.Users.FirstOrDefault(u => u.Email == txtUsername.Text && u.Password == txtPassword.Password);

            if (currentUser != null)
            {
                Application.Current.Properties["Email"] = currentUser.Email;
                Application.Current.Properties["Name"] = currentUser.UserName;
                Application.Current.Properties["DateOfBirth"] = currentUser.DateOfBirth;
                Application.Current.Properties["Phone"] = currentUser.Phone;
                Application.Current.Properties["Addres"] = currentUser.Address;
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                Hide();
            }
            else
            {
                MessageBox.Show("Неверное имя пользователя или пароль. \nПожалуйста, попробуйте еще раз.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                txtUsername.Text = "";
                txtPassword.Password = "";
                txtUsername.Focus();
            }
        }

        private void SignupBtn_Click(object sender, RoutedEventArgs e)
        {
            Registration registration = new Registration();
            registration.Show();
            Hide();
        }
    }
}
