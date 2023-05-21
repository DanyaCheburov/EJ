using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EJ.Profile
{
    /// <summary>
    /// Логика взаимодействия для AccountProfile.xaml
    /// </summary>
    public partial class AccountProfile : Page
    {
        public AccountProfile()
        {
            InitializeComponent();
            UserData();
        }
        public void UserData()
        {
            string userEmail = Application.Current.Properties["Email"] as string;
            EmailTextBlock.Text = userEmail;
            DateTime? dateOfBirth = Application.Current.Properties["DateOfBirth"] as DateTime?;
            if (dateOfBirth.HasValue)
            {
                DateOfBirthTextBlock.Text = dateOfBirth.Value.ToString("dd.MM.yyyy");
            }
            else
            {
                DateOfBirthTextBlock.Text = "";
            }

            string phone = Application.Current.Properties["Phone"] as string;
            if (!string.IsNullOrEmpty(phone))
            {
                PhoneTextBlock.Text = phone;
            }
            else
            {
                PhoneTextBlock.Text = "";
            }

            string address = Application.Current.Properties["Addres"] as string;
            if (!string.IsNullOrEmpty(address))
            {
                AddresTextBlock.Text = address;
            }
            else
            {
                AddresTextBlock.Text = "";
            }
            string userName = Application.Current.Properties["Name"] as string;
            string[] nameParts = userName.Split(' ');
            if (nameParts.Length >= 3)
            {
                string shortName = nameParts[0] + " " + nameParts[1][0] + "." + nameParts[2][0] + ".";
                NameTextBlock.Text = shortName;
            }
            else if (nameParts.Length >= 2)
            {
                string shortName = nameParts[0] + " " + nameParts[1][0] + ".";
                NameTextBlock.Text = shortName;
            }
            else if (nameParts.Length >= 1)
            {
                string shortName = nameParts[0];
                NameTextBlock.Text = shortName;
            }
            else
            {
                NameTextBlock.Text = userName;
            }
        }
        private void ProfileUpdate_Click(object sender, RoutedEventArgs e)
        {
            ProfileUpdate profileUpdatePage = new ProfileUpdate(this);
            MainContentFrameProfile.Navigate(profileUpdatePage);
        }
    }
}
