using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace EJ.MainMenu.Editing_information
{
    /// <summary>
    /// Логика взаимодействия для AddEditUser.xaml
    /// </summary>
    public partial class AddEditUser : Window
    {
        private Users _currentUser = new Users();
        public AddEditUser(Users selectedUser)
        {
            InitializeComponent();
            if (selectedUser != null)
                _currentUser = selectedUser;

            DataContext = _currentUser;
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
        private void EditUserSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentUser.UserName))
                errors.AppendLine("Укажите ФИО.");
            if (string.IsNullOrWhiteSpace(_currentUser.Email))
                errors.AppendLine("Укажите почту.");
            if (string.IsNullOrWhiteSpace(_currentUser.Password))
                errors.AppendLine("Укажите пароль.");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_currentUser.UserId == 0)
            {
                string hashedPassword = HashPassword(_currentUser.Password);
                _currentUser.Password = hashedPassword;
                BDEntities.GetContext().Users.Add(_currentUser);
            }
            else
            {
                string hashedPassword = HashPassword(_currentUser.Password);
                _currentUser.Password = hashedPassword;
            }

            try
            {
                BDEntities.GetContext().SaveChanges();
                MessageBox.Show("Информация сохранена!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
