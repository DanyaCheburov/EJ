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
            ComboStudent.ItemsSource = BDEntities.GetContext().Students.ToList();
            ComboGroup.ItemsSource = BDEntities.GetContext().Groups.Local;
        }

        private void add_attendance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new BDEntities())
                {
                    var entity = ComboStudent.SelectedItem as Students;
                    context.Entry(entity).State = EntityState.Detached;

                    using (SqlConnection connection = new SqlConnection(@"Data Source=localhost\SQLEXPRESS1;Initial Catalog=BD;Integrated Security=True"))
                    {
                        connection.Open();

                        using (SqlCommand command = new SqlCommand("INSERT INTO Attendance (StudentId, Attended , Date, SubjectId) VALUES (@Value, @Value1,  @Value2, @Value3)", connection))
                        {
                            if (datePicker.SelectedDate != null && entity != null)
                            {
                                command.Parameters.AddWithValue("@Value", entity.Id);
                                command.Parameters.AddWithValue("@Value1", 0);
                                command.Parameters.AddWithValue("@Value2", datePicker.SelectedDate.Value.ToString("yyyy-MM-dd"));
                                command.Parameters.AddWithValue("@Value3", 1);
                                command.ExecuteNonQuery();
                                MessageBox.Show("Data saved successfully.");
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
    }
}
