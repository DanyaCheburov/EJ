using System;
using System.Windows;
using System.Windows.Input;

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

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
    }
}
