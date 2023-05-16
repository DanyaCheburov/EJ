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

namespace EJ.AttendanceManagement
{
    /// <summary>
    /// Логика взаимодействия для PassManagement.xaml
    /// </summary>
    public partial class PassManagement : Window
    {
        public PassManagement()
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

        private void Add_attendance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new BDEntities())
                {
                    var entity = ComboStudent.SelectedItem as Students;
                    var entity2 = ComboSubject.SelectedItem as Subjects;
                    context.Entry(entity).State = EntityState.Detached;

                    var attendance = context.Attendance.FirstOrDefault(a => a.StudentId == entity.StudentId && a.Date == datePicker.SelectedDate.Value && a.SubjectId == entity2.SubjectId);

                    if (attendance != null)
                    {
                        bool previousPassType = attendance.PassType;
                        bool newPassType = false;

                        if (ComboPassType.SelectedIndex == 0) // Отсутствие по неуважительной причине
                        {
                            newPassType = false;
                        }
                        else if (ComboPassType.SelectedIndex == 1) // Отсутствие по уважительной причине
                        {
                            newPassType = true;
                        }
                        else if (ComboPassType.SelectedIndex == 2) // Удаление пропуска
                        {
                            context.Attendance.Remove(attendance);
                            context.SaveChanges();
                            MessageBox.Show("Пропуск удален.");
                            return;
                        }

                        if (previousPassType != newPassType) // Если тип пропуска изменился
                        {
                            attendance.PassType = newPassType;
                            context.SaveChanges();
                            MessageBox.Show("Пропуск обновлен.");
                        }
                        else
                        {
                            MessageBox.Show("Пропуск уже существует.");
                        }
                    }
                    else
                    {
                        if (ComboPassType.SelectedIndex == 2) // Удаление пропуска, но его нет
                        {
                            MessageBox.Show("Пропуска нет, чтобы его удалить.");
                            return;
                        }

                        if (datePicker.SelectedDate != null && entity != null)
                        {
                            bool passType = false;

                            if (ComboPassType.SelectedIndex == 0) // Отсутствие по неуважительной причине
                            {
                                passType = false;
                            }
                            else if (ComboPassType.SelectedIndex == 1) // Отсутствие по уважительной причине
                            {
                                passType = true;
                            }

                            Attendance newAttendance = new Attendance()
                            {
                                StudentId = entity.StudentId,
                                Date = datePicker.SelectedDate.Value,
                                SubjectId = entity2.SubjectId,
                                PassType = passType
                            };

                            context.Attendance.Add(newAttendance);
                            context.SaveChanges();
                            MessageBox.Show("Пропуск добавлен.");
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

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

