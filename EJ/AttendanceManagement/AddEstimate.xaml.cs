using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EJ.AttendanceManagement
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
            this.Close();
        }

        private void Add_estimate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new BDEntities())
                {
                    var selectedStudent = ComboStudent.SelectedItem as Students;
                    var selectedSubject = ComboSubject.SelectedItem as Subjects;

                    var existingRecord = context.Journal
                        .FirstOrDefault(j => j.StudentId == selectedStudent.StudentId &&
                                             j.SubjectId == selectedSubject.SubjectId &&
                                             j.Date == datePicker.SelectedDate.Value);

                    if (existingRecord != null)
                    {
                        int previousEstimate = existingRecord.Estimate;
                        int newEstimate = Convert.ToInt32(((ComboBoxItem)ComboEstimate.SelectedItem).Content);

                        if (previousEstimate != newEstimate)
                        {
                            existingRecord.Estimate = newEstimate;
                            context.SaveChanges();
                            MessageBox.Show("Оценка обновлена.");
                        }
                        else
                        {
                            MessageBox.Show("Оценка уже существует.");
                        }
                    }
                    else
                    {
                        if (datePicker.SelectedDate != null && selectedStudent != null)
                        {
                            int estimate = Convert.ToInt32(((ComboBoxItem)ComboEstimate.SelectedItem).Content);

                            var newRecord = new Journal
                            {
                                StudentId = selectedStudent.StudentId,
                                SubjectId = selectedSubject.SubjectId,
                                Date = datePicker.SelectedDate.Value,
                                Estimate = estimate
                            };

                            context.Journal.Add(newRecord);
                            context.SaveChanges();
                            MessageBox.Show("Оценка добавлена.");
                        }
                        else
                        {
                            MessageBox.Show("Пожалуйста, выберите действительного студента и дату из комбо-боксов.");
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