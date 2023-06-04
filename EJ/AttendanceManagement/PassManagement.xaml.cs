using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EJ.AttendanceManagement
{
    public partial class PassManagement : Window
    {
        private readonly BDEntities _context = new BDEntities();
        public string SelectedGroup { get; set; }
        public string SelectedSubject { get; set; }

        public PassManagement(string selectedGroup, string selectedSubject)
        {
            InitializeComponent();
            SelectedGroup = selectedGroup;
            ComboGroup.ItemsSource = _context.Groups.ToList();
            ComboGroup.SelectedItem = _context.Groups.FirstOrDefault(g => g.GroupName == SelectedGroup);

            SelectedSubject = selectedSubject;
            ComboSubject.ItemsSource = _context.Subjects.ToList();
            ComboSubject.SelectedItem = _context.Subjects.FirstOrDefault(s => s.SubjectName == SelectedSubject);

            ListBoxStudents.ItemsSource = Students;
        }

        private List<Students> Students
        {
            get
            {
                if (ComboGroup.SelectedItem is Groups selectedGroup)
                {
                    return _context.Students.Where(s => s.GroupId == selectedGroup.GroupId).ToList();
                }
                return null;
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
        private void OpenStudentsButton_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsPopup.IsOpen)
            {
                StudentsPopup.IsOpen = false; // Закрыть Popup
            }
            else
            {
                StudentsPopup.IsOpen = true; // Открыть Popup
            }
        }
        private void ComboGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBoxStudents.ItemsSource = Students;
        }

        private void Add_attendance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedSubject = ComboSubject.SelectedItem as Subjects;

                if (selectedSubject == null || datePicker.SelectedDate == null)
                {
                    MessageBox.Show("Пожалуйста, выберите дату из комбо-боксов.");
                    return;
                }

                bool passType = ComboPassType.SelectedIndex == 1; // Отсутствие по уважительной причине

                foreach (var selectedStudent in ListBoxStudents.SelectedItems.Cast<Students>())
                {
                    var attendance = _context.Attendance.FirstOrDefault(a => a.StudentId == selectedStudent.StudentId && a.Date == datePicker.SelectedDate.Value && a.SubjectId == selectedSubject.SubjectId);

                    if (ComboPassType.SelectedIndex == 2) // Удаление пропуска
                    {
                        if (attendance != null)
                        {
                            _context.Attendance.Remove(attendance);
                            MessageBox.Show("Пропуск для студента " + selectedStudent.Users.UserName + " удален.");
                        }
                        else
                        {
                            MessageBox.Show("Пропуска нет, чтобы его удалить для студента " + selectedStudent.Users.UserName + ".");
                        }
                    }
                    else
                    {
                        if (attendance != null)
                        {
                            if (attendance.PassType == passType)
                            {
                                MessageBox.Show("Пропуск уже существует для студента " + selectedStudent.Users.UserName + ".");
                            }
                            else
                            {
                                attendance.PassType = passType;
                                MessageBox.Show("Пропуск для студента " + selectedStudent.Users.UserName + " обновлен.");
                            }
                        }
                        else
                        {
                            Attendance newAttendance = new Attendance()
                            {
                                StudentId = selectedStudent.StudentId,
                                Date = datePicker.SelectedDate.Value,
                                SubjectId = selectedSubject.SubjectId,
                                PassType = passType
                            };

                            _context.Attendance.Add(newAttendance);
                            MessageBox.Show("Пропуск для студента " + selectedStudent.Users.UserName + " добавлен.");
                        }
                    }
                }

                _context.SaveChanges();
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
