using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Security.Cryptography;
using System.Text;
using System;

namespace EJ.AuthorizationANDRegistration
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        public Registration()
        {
            InitializeComponent();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            Authorization authorization = new Authorization();
            authorization.Show();
            Hide();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            regtxtFIO.Text = "";
            regtxtEmail.Text = "";
            regtxtPassword.Password = "";
            regconfrimtxtPassword.Password = "";
            regtxtFIO.Focus();
        }

        private void RegisterBtn_Click(object sender, RoutedEventArgs e)
        {
            string fio = regtxtFIO.Text.Trim();
            string email = regtxtEmail.Text.Trim();
            string password = regtxtPassword.Password.Trim();
            string confirmPassword = regconfrimtxtPassword.Password.Trim();

            Regex fioRegex = new Regex("^[А-ЯЁ][а-яё]+\\s[А-ЯЁ][а-яё]+(\\s[А-ЯЁ][а-яё]+)?$");
            if (!fioRegex.IsMatch(fio))
            {
                MessageBox.Show("Введите ФИО в правильном формате (например, Иванов Иван Иванович)!");
                return;
            }
            if (password != confirmPassword)
            {
                MessageBox.Show("Пароль и подтверждение пароля не совпадают!");
                return;
            }

            string hashedPassword = HashPassword(password);

            using (var context = new BDEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    MessageBox.Show("Пользователь с таким Email уже существует!");
                    return;
                }
            }
            using (var context = new BDEntities())
            {
                var user = new Users
                {
                    UserName = fio,
                    Email = email,
                    Password = hashedPassword
                };
                context.Users.Add(user);
                context.SaveChanges();
            }
            MessageBox.Show("Регистрация прошла успешно!");
            Authorization authorization = new Authorization();
            authorization.Show();
            Hide();
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
