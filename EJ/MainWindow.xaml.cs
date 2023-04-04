using System;
using System.Collections.Generic;
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

namespace EJ
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new Uri("MainMenu/UsersPage.xaml", UriKind.Relative));
        }

        private void StudentButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new Uri("MainMenu/StudentsPage.xaml", UriKind.Relative));
        }

        private void TeacherButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new Uri("MainMenu/TeachersPage.xaml", UriKind.Relative));
        }

        private void DepartmentButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new Uri("MainMenu/AttedancePage.xaml", UriKind.Relative));
        }
    }
}
