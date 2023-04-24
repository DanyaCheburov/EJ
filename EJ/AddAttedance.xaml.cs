using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
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
    /// Логика взаимодействия для AddAttedance.xaml
    /// </summary>
    public partial class AddAttedance : Window
    {
        public AddAttedance()
        {
            InitializeComponent();
            ComboGroup.ItemsSource = BDEntities.GetContext().Groups.Local;
            ComboSubject.ItemsSource = BDEntities.GetContext().Subjects.ToList();
            ComboStudent.ItemsSource = Students;
        }

        private List<Students> Students
        {
            get
            {
                if (ComboGroup.SelectedItem != null)
                {
                    var selectedGroup = ComboGroup.SelectedItem as Groups;
                    return BDEntities.GetContext().Students.Where(s => s.GroupId == selectedGroup.GroupId).ToList();
                }
                return null;
            }
        }

        private void ComboGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboStudent.ItemsSource = Students;
        }

        private void Add_attendance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new BDEntities())
                {
                    var entity = ComboStudent.SelectedItem as Students;
                    var entity2 = ComboSubject.SelectedItem as Subjects;
                    context.Entry(entity).State = EntityState.Detached;

                    using (SqlConnection connection = new SqlConnection(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=BD;Integrated Security=True"))
                    {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand("INSERT INTO Attendance (StudentId, Date, SubjectId) VALUES (@Value,  @Value1, @Value2)", connection))
                        {
                            if (datePicker.SelectedDate != null && entity != null)
                            {
                                command.Parameters.AddWithValue("@Value", entity.Id);
                                command.Parameters.AddWithValue("@Value1", datePicker.SelectedDate.Value.ToString("yyyy-MM-dd"));
                                command.Parameters.AddWithValue("@Value2", entity2.SubjectId);
                                command.ExecuteNonQuery();
                                MessageBox.Show("Пропуск добавлен.");
                            }
                            else
                            {
                                MessageBox.Show("Please select a valid student and date from the comboboxes.");
                            }
                            connection.Close();
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message);
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}
