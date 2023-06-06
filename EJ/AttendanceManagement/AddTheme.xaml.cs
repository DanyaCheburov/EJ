using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EJ.AttendanceManagement
{
    /// <summary>
    /// Логика взаимодействия для AddTheme.xaml
    /// </summary>
    public partial class AddTheme : Window
    {
        public AddTheme()
        {
            InitializeComponent();
            ComboGroup.ItemsSource = BDEntities.GetContext().Groups.Local;
            ComboSubject.ItemsSource = BDEntities.GetContext().Subjects.ToList();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Add_theme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new BDEntities())
                {
                    var selectedSubject = ComboSubject.SelectedItem as Subjects;
                    var selectedGroup = ComboGroup.SelectedItem as Groups;

                    var existingRecord = context.Lesson_themes
                        .FirstOrDefault(t => t.SubjectId == selectedSubject.SubjectId &&
                                             t.Date == datePicker.SelectedDate.Value);

                    if (existingRecord != null)
                    {
                        string previousEstimate = existingRecord.Description;
                        string newTheme = Convert.ToString((DescriptionText.Text));

                        if (previousEstimate != newTheme)
                        {
                            existingRecord.Description = newTheme;
                            context.SaveChanges();
                            MessageBox.Show("Тема обновлена.");
                        }
                        else
                        {
                            MessageBox.Show("Тема уже существует.");
                        }
                    }
                    else
                    {
                        if (datePicker.SelectedDate != null && DescriptionText != null)
                        {
                            string description = Convert.ToString(DescriptionText.Text);

                            var newRecord = new Lesson_themes
                            {
                                GroupId=selectedGroup.GroupId,
                                SubjectId = selectedSubject.SubjectId,
                                Date = datePicker.SelectedDate.Value,
                                Description = description
                            };

                            context.Lesson_themes.Add(newRecord);
                            context.SaveChanges();
                            MessageBox.Show("Тема добавлена.");
                        }
                        else
                        {
                            MessageBox.Show("Пожалуйста, выберите действительный студента и дату из комбо-боксов.");
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
