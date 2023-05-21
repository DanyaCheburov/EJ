using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Security.Cryptography;
using System.Text;
using System;

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
            string email = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            using (var context = new BDEntities())
            {
                var currentUser = context.Users.FirstOrDefault(u => u.Email == email);

                if (currentUser != null && VerifyPassword(password, currentUser.Password))
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
        }

        private bool VerifyPassword(string enteredPassword, string hashedPassword)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));
                string enteredPasswordHash = Convert.ToBase64String(hashedBytes);
                return hashedPassword == enteredPasswordHash;
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
