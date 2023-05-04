using EJ.MainMenu;
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
                MainWindow mainWindow = new MainWindow(currentUser.UserName);
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
