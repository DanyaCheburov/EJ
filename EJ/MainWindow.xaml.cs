using EJ.AuthorizationANDRegistration;
using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EJ
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool IsDarkTheme { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            string userName = (string)Application.Current.Properties["Name"];
            myChip.Content = userName;
            StateChanged += Window_StateChanged;
            AdminRole();
            StudentRole();
        }

        private void AdminRole()
        {
            // Получение информации о текущем пользователе из БД
            int currentUser = (int)Application.Current.Properties["UserId"];

            // Проверка, является ли текущий пользователь администратором
            bool isAdmin = IsUserAdmin(currentUser);
            bool isTeacher = IsUserTeacher(currentUser);

            // Установка видимости кнопки
            if (isAdmin)
            {
                UserButton.Visibility = Visibility.Visible;
                StudentButton.Visibility = Visibility.Visible;
            }
            else
            {
                UserButton.Visibility = Visibility.Collapsed;
                StudentButton.Visibility = Visibility.Collapsed;
            }
            if (isAdmin || isTeacher)
            {
                AttendanceButton.Visibility = Visibility.Visible;
            }
            else
            {
                AttendanceButton.Visibility = Visibility.Collapsed;
            }

            // Метод для проверки, является ли пользователь администратором
            bool IsUserAdmin(int userId)
            {
                using (var context = new BDEntities())
                {
                    // Проверка наличия пользователя с заданным UserId в таблице Administrators
                    isAdmin = context.Administrators.Any(a => a.UserId == userId);
                    return isAdmin;
                }
            }
            bool IsUserTeacher(int userId)
            {
                using (var context = new BDEntities())
                {
                    isTeacher = context.Teachers.Any(a => a.UserId == userId);
                    return isTeacher;
                }
            }
        }
        private void StudentRole()
        {
            int currentUser = (int)Application.Current.Properties["UserId"];

            // Проверка, является ли текущий пользователь администратором
            bool isStudent = IsUserStudent(currentUser);

            // Установка видимости кнопки
            if (isStudent)
            {
                AttendanceStudentButton.Visibility = Visibility.Visible;
            }
            else
            {
                AttendanceStudentButton.Visibility = Visibility.Collapsed;
            }

            // Метод для проверки, является ли пользователь администратором
            bool IsUserStudent(int userId)
            {
                using (var context = new BDEntities())
                {
                    isStudent = context.Students.Any(a => a.UserId == userId);
                    return isStudent;
                }
            }
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                myBorder.Height = Double.NaN;
            }
            else
            {
                myBorder.Height = 610;
            }
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

        private void AttendanceButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new Uri("MainMenu/AttendanceTeacherPage.xaml", UriKind.Relative));
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


        private readonly PaletteHelper paletteHelper = new PaletteHelper();
        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            ITheme theme = paletteHelper.GetTheme();

            if (IsDarkTheme = theme.GetBaseTheme() == BaseTheme.Dark)
            {
                IsDarkTheme = false;
                theme.SetBaseTheme(Theme.Light);
                UserButton.Foreground = Brushes.Black; // устанавливаем цвет текста на черный для светлой темы
                AttendanceButton.Foreground = Brushes.Black;
                StudentButton.Foreground = Brushes.Black;
                TeacherButton.Foreground = Brushes.Black;
                HelpButton.Foreground = Brushes.Black;
                EstimateButton.Foreground = Brushes.Black;
                AttendanceStudentButton.Foreground = Brushes.Black;
                themeToggle.ToolTip = "Включить темную тему";
            }
            else
            {
                IsDarkTheme = true;
                theme.SetBaseTheme(Theme.Dark);
                UserButton.Foreground = Brushes.White; // устанавливаем цвет текста на белый для темной темы
                AttendanceButton.Foreground = Brushes.White;
                StudentButton.Foreground = Brushes.White;
                TeacherButton.Foreground = Brushes.White;
                EstimateButton.Foreground = Brushes.White;
                HelpButton.Foreground = Brushes.White;
                AttendanceStudentButton.Foreground = Brushes.White;
                themeToggle.ToolTip = "Включить светлую тему";
            }
            paletteHelper.SetTheme(theme);
        }

        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MyChip_Click(object sender, RoutedEventArgs e)
        {
            myPopup.IsPopupOpen = true;
        }
        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new Uri("Profile/AccountProfile.xaml", UriKind.Relative));
        }

        private void LeaveProfile_Click(object sender, RoutedEventArgs e)
        {
            // Включаем светлую тему
            ITheme theme = paletteHelper.GetTheme();
            IsDarkTheme = false;
            theme.SetBaseTheme(Theme.Light);
            UserButton.Foreground = Brushes.Black;
            AttendanceButton.Foreground = Brushes.Black;
            StudentButton.Foreground = Brushes.Black;
            TeacherButton.Foreground = Brushes.Black;
            HelpButton.Foreground = Brushes.Black;
            EstimateButton.Foreground = Brushes.Black;
            paletteHelper.SetTheme(theme);
            this.Close();

            Authorization authWindow = new Authorization();
            authWindow.Show();
        }

        private void EstimateButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new Uri("MainMenu/JournalPage.xaml", UriKind.Relative));
        }

        private void AttendanceStudentButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new Uri("MainMenu/AttendanceStudentPage.xaml", UriKind.Relative));
        }
    }
}
