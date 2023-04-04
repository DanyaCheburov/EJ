using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EJ.MainMenu
{
    public partial class UsersPage : Page
    {
        public ObservableCollection<Users> Users { get; set; }

        public UsersPage()
        {
            InitializeComponent();

            // Создание нового объекта контекста базы данных
            using (var db = new BDEntities())
            {
                // Загрузка всех пользователей из таблицы Users
                var users = db.Users.ToList();

                // Конвертирование списка пользователей в ObservableCollection
                Users = new ObservableCollection<Users>(users);
            }

            // Установка контекста данных для DataGrid
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
