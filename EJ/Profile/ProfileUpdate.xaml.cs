using DocumentFormat.OpenXml.Drawing.Diagrams;
using System;
using System.Data.SqlClient;
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
        public ProfileUpdate()
        {
            InitializeComponent();
            DateTime? dateOfBirth = Application.Current.Properties["DateOfBirth"] as DateTime?;
            if (dateOfBirth.HasValue)
            {
                DateOfBirthTextBox.Text = dateOfBirth.Value.ToString("dd.MM.yyyy");
            }
            else
            {
                DateOfBirthTextBox.Text = "";
            }

            string phone = Application.Current.Properties["Phone"] as string;
            if (!string.IsNullOrEmpty(phone))
            {
                PhoneTextBox.Text = phone;
            }
            else
            {
                PhoneTextBox.Text = "";
            }

            string address = Application.Current.Properties["Addres"] as string;
            if (!string.IsNullOrEmpty(address))
            {
                AddresTextBox.Text = address;
            }
            else
            {
                AddresTextBox.Text = "";
            }
            string userName = Application.Current.Properties["Name"] as string;
            NameTextBox.Text = userName;
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
                // Создаем объект подключения
                using (SqlConnection connection = new SqlConnection(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=BD;Integrated Security=True"))
                {
                    // Открываем соединение
                    connection.Open();

                    // Создаем SQL-запрос на обновление данных пользователя
                    string query = "UPDATE Users SET UserName = @UserName, Email = @Email,  DateOfBirth = @DateOfBirth, Phone = @Phone, Address = @Address";

                    // Создаем объект команды с параметрами
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", userName);
                        command.Parameters.AddWithValue("@DateOfBirth", dateOfBirth);
                        command.Parameters.AddWithValue("@Phone", phone);
                        command.Parameters.AddWithValue("@Address", address);

                        // Выполняем команду
                        command.ExecuteNonQuery();
                    }

                    // Закрываем соединение
                    connection.Close();
                }
                MessageBox.Show("Данные успешно сохранены.");
            }
            else
            {
                MessageBox.Show("Проверьте правильность введенных данных.");
            }
        }
    }
}
