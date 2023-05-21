using EJ.MainMenu.Editing_information;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EJ.MainMenu
{
    public partial class UsersPage : Page
    {
        public UsersPage()
        {
            InitializeComponent();
            usersDataGrid.ItemsSource = BDEntities.GetContext().Users.ToList();
        }

        public void Refresh()
        {
            usersDataGrid.ItemsSource = BDEntities.GetContext().Users.ToList();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var edituser = new AddEditUser((sender as Button).DataContext as Users);
            edituser.ShowDialog();
            Refresh();
        }
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var adduser = new AddEditUser(null);
            adduser.ShowDialog();
            Refresh();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var userForRemoving = usersDataGrid.SelectedItems.Cast<Users>().ToList();

            if (MessageBox.Show($"Вы точно хотите удалить следующие {userForRemoving.Count()} элементов?", "Внимание",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    BDEntities.GetContext().Users.RemoveRange(userForRemoving);
                    BDEntities.GetContext().SaveChanges();
                    MessageBox.Show("Данные удалены!");

                    usersDataGrid.ItemsSource = BDEntities.GetContext().Users.ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
        }
    }
}
