using EJ.AuthorizationANDRegistration;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace EJ
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _username;
        public MainWindow(string username)
        {
            InitializeComponent();
            _username = username;
            myChip.Content = _username;
            StateChanged += Window_StateChanged;
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

        public bool IsDarkTheme { get; set; }

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
            // Закрыть текущее окно MainWindow
            this.Close();

            // Создать и открыть новое окно Authorization
            Authorization authWindow = new Authorization();
            authWindow.Show();
        }

        private void EstimateButton_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(new Uri("MainMenu/JournalPage.xaml", UriKind.Relative));
        }
    }
}
