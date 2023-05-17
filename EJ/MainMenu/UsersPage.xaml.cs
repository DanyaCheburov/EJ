using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EJ.MainMenu
{
    public partial class UsersPage : Page
    {
        public ObservableCollection<Users> Users { get; set; }

        public UsersPage()
        {
            InitializeComponent();

            using (var db = new BDEntities())
            {
                var users = db.Users.ToList();
                Users = new ObservableCollection<Users>(users);
            }
            DataContext = this;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new BDEntities())
            {
                foreach (var user in Users)
                {
                    db.Entry(user).State = EntityState.Modified;
                }
                db.SaveChanges();
            }
        }
    }
}
