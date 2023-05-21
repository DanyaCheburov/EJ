using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace EJ.Profile
{
    /// <summary>
    /// Логика взаимодействия для ProfileUpdate.xaml
    /// </summary>
    public partial class ProfileUpdate : Page
    {
        private AccountProfile accountProfile;
        public ProfileUpdate(AccountProfile accountProfile)
        {
            InitializeComponent();
            ProfileUserData();
            this.accountProfile = accountProfile;
        }

        public void ProfileUserData()
        {
            // Получаем данные пользователя из свойств приложения
            string userName = (string)Application.Current.Properties["Name"];
            DateTime? dateOfBirth = Application.Current.Properties["DateOfBirth"] as DateTime?;
            string phone = (string)Application.Current.Properties["Phone"];
            string address = (string)Application.Current.Properties["Addres"];

            // Заполняем текстовые поля на странице профиля
            NameTextBox.Text = userName;
            if (dateOfBirth.HasValue)
            {
                DateOfBirthTextBox.Text = dateOfBirth.Value.ToString("dd.MM.yyyy");
            }
            else
            {
                DateOfBirthTextBox.Text = "";
            }
            PhoneTextBox.Text = phone ?? "";
            AddresTextBox.Text = address ?? "";
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем данные пользователя из текстовых полей
            string userName = NameTextBox.Text.Trim();
            string dateOfBirth = DateOfBirthTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();
            string address = AddresTextBox.Text.Trim();
            // Проверяем правильность формата ФИО
            bool isNameValid = Regex.IsMatch(userName, @"^[А-ЯЁ][а-яё]*(\s[А-ЯЁ][а-яё]*)*$");

            // Проверяем правильность формата даты рождения
            bool isDateOfBirthValid = Regex.IsMatch(dateOfBirth, @"^(0[1-9]|[1-2][0-9]|3[0-1])\.(0[1-9]|1[0-2])\.(19|20)[0-9]{2}$");

            if (isNameValid && isDateOfBirthValid)
            {
                // Получаем email текущего пользователя из свойства приложения
                string userEmail = (string)Application.Current.Properties["Email"];

                // Получаем данные пользователя из БД
                var currentUser = BDEntities.db.Users.FirstOrDefault(u => u.Email == userEmail);

                // Обновляем данные пользователя в базе данных
                currentUser.UserName = userName;
                currentUser.DateOfBirth = DateTime.ParseExact(dateOfBirth, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                currentUser.Phone = phone;
                currentUser.Address = address;
                BDEntities.db.SaveChanges();

                // Обновляем данные в свойствах приложения
                Application.Current.Properties["Name"] = userName;
                Application.Current.Properties["DateOfBirth"] = currentUser.DateOfBirth;
                Application.Current.Properties["Phone"] = phone;
                Application.Current.Properties["Addres"] = address;

                accountProfile.UserData();
                MessageBox.Show("Данные успешно сохранены.");
            }
            else
            {
                MessageBox.Show("Проверьте правильность введенных данных.");
            }
        }
    }
}
