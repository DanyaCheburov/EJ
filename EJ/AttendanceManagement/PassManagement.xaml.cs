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

            ComboStudent.ItemsSource = Students;
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

        private void ComboGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboStudent.ItemsSource = Students;
        }

        private void Add_attendance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedStudent = ComboStudent.SelectedItem as Students;
                var selectedSubject = ComboSubject.SelectedItem as Subjects;

                if (selectedStudent == null || selectedSubject == null || datePicker.SelectedDate == null)
                {
                    MessageBox.Show("Пожалуйста, выберите действительного студента и дату из комбо-боксов.");
                    return;
                }

                var attendance = _context.Attendance.FirstOrDefault(a => a.StudentId == selectedStudent.StudentId && a.Date == datePicker.SelectedDate.Value && a.SubjectId == selectedSubject.SubjectId);

                if (ComboPassType.SelectedIndex == 2) // Удаление пропуска
                {
                    if (attendance != null)
                    {
                        _context.Attendance.Remove(attendance);
                        _context.SaveChanges();
                        MessageBox.Show("Пропуск удален.");
                    }
                    else
                    {
                        MessageBox.Show("Пропуска нет, чтобы его удалить.");
                    }
                    return;
                }

                bool passType = ComboPassType.SelectedIndex == 1; // Отсутствие по уважительной причине

                if (attendance != null)
                {
                    if (attendance.PassType == passType)
                    {
                        MessageBox.Show("Пропуск уже существует.");
                        return;
                    }

                    attendance.PassType = passType;
                    _context.SaveChanges();
                    MessageBox.Show("Пропуск обновлен.");
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
                    _context.SaveChanges();
                    MessageBox.Show("Пропуск добавлен.");
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
