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
using System.Windows.Shapes;

namespace EJ
{
    /// <summary>
    /// Логика взаимодействия для AddEstimate.xaml
    /// </summary>
    public partial class AddEstimate : Window
    {
        public AddEstimate()
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

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void ComboGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboStudent.ItemsSource = Students;
        }
        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void add_estimate_Click(object sender, RoutedEventArgs e)
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

                        using (SqlCommand command = new SqlCommand("SELECT * FROM Journal WHERE StudentId = @Value AND SubjectId = @Value1 AND Date = @Value2", connection))
                        {
                            command.Parameters.AddWithValue("@Value", entity.StudentId);
                            command.Parameters.AddWithValue("@Value1", entity2.SubjectId);
                            command.Parameters.AddWithValue("@Value2", datePicker.SelectedDate.Value.ToString("yyyy-MM-dd"));

                            SqlDataReader reader = command.ExecuteReader();

                            if (reader.Read())
                            {
                                int previousEstimate = reader.GetInt32(reader.GetOrdinal("Estimate"));
                                int newEstimate = Convert.ToInt32(((ComboBoxItem)ComboEstimate.SelectedItem).Content);

                                if (previousEstimate != newEstimate) // Если оценка изменилась
                                {
                                    reader.Close();

                                    using (SqlCommand updateCommand = new SqlCommand("UPDATE Journal SET Estimate = @Value3 WHERE StudentId = @Value AND SubjectId = @Value1 AND Date = @Value2", connection))
                                    {
                                        updateCommand.Parameters.AddWithValue("@Value", entity.StudentId);
                                        updateCommand.Parameters.AddWithValue("@Value1", entity2.SubjectId);
                                        updateCommand.Parameters.AddWithValue("@Value2", datePicker.SelectedDate.Value.ToString("yyyy-MM-dd"));
                                        updateCommand.Parameters.AddWithValue("@Value3", newEstimate);
                                        updateCommand.ExecuteNonQuery();
                                        MessageBox.Show("Оценка обновлена.");
                                    }
                                }
                                else
                                {
                                    reader.Close();
                                    MessageBox.Show("Оценка уже существует.");
                                }
                            }
                            else
                            {
                                reader.Close();

                                using (SqlCommand insertCommand = new SqlCommand("INSERT INTO Journal (StudentId, SubjectId, Date, Estimate) VALUES (@Value,  @Value1, @Value2, @Value3)", connection))
                                {
                                    if (datePicker.SelectedDate != null && entity != null)
                                    {
                                        int estimate = Convert.ToInt32(((ComboBoxItem)ComboEstimate.SelectedItem).Content);

                                        insertCommand.Parameters.AddWithValue("@Value", entity.StudentId);
                                        insertCommand.Parameters.AddWithValue("@Value1", entity2.SubjectId);
                                        insertCommand.Parameters.AddWithValue("@Value2", datePicker.SelectedDate.Value.ToString("yyyy-MM-dd"));
                                        insertCommand.Parameters.AddWithValue("@Value3", estimate);
                                        insertCommand.ExecuteNonQuery();
                                        MessageBox.Show("Оценка добавлена.");
                                    }
                                    else
                                    {
                                        MessageBox.Show("Пожалуйста, выберите действительного студента и дату из комбо-боксов.");
                                    }
                                }
                            }

                            connection.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения данных: " + ex.Message);
            }
        }
    }
}