using EJ.MainMenu.Editing_information;
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
            var addedituser = new AddEditUser((sender as Button).DataContext as Users);
            addedituser.ShowDialog();
            Refresh();
        }
    }
}
