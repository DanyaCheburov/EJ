using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            // получить данные пользователя
            string fio = regtxtFIO.Text.Trim();
            string email = regtxtEmail.Text.Trim();
            string password = regtxtPassword.Password.Trim();
            string confirmPassword = regconfrimtxtPassword.Password.Trim();

            // проверить, что ФИО введено в правильном формате
            Regex fioRegex = new Regex("^[А-ЯЁ][а-яё]+\\s[А-ЯЁ][а-яё]+(\\s[А-ЯЁ][а-яё]+)?$");
            if (!fioRegex.IsMatch(fio))
            {
                MessageBox.Show("Введите ФИО в правильном формате (например, Иванов Иван Иванович)!");
                return;
            }

            // проверить, что пароль и подтверждение пароля совпадают
            if (password != confirmPassword)
            {
                MessageBox.Show("Пароль и подтверждение пароля не совпадают!");
                return;
            }

            // проверить, что пользователь с таким email не существует
            using (var context = new BDEntities())
            {
                var user = context.Users.FirstOrDefault(u => u.Email == email);
                if (user != null)
                {
                    MessageBox.Show("Пользователь с таким Email уже существует!");
                    return;
                }
            }

            // добавить нового пользователя в базу данных
            using (var context = new BDEntities())
            {
                var user = new Users
                {
                    UserName = fio,
                    Email = email,
                    Password = password
                };
                context.Users.Add(user);
                context.SaveChanges();
            }

            // сообщение об успешной регистрации
            MessageBox.Show("Регистрация прошла успешно!");
            Authorization authorization = new Authorization();
            authorization.Show();
            Hide();
        }
    }
}
