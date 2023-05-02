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

namespace EJ.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для Profile.xaml
    /// </summary>
    public partial class Profile : Page
    {
        public Profile()
        {
            InitializeComponent();
            string userEmail = Application.Current.Properties["Email"] as string;
            EmailTextBlock.Text = userEmail;
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
    }
}
