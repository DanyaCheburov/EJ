using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Windows.Controls;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;


namespace EJ.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для AttedancePage.xaml
    /// </summary>
    public partial class AttedancePage : Page
    {
        public ObservableCollection<Students> Students { get; set; }
        public AttedancePage()
        {
            InitializeComponent();
            CreateTable();
            ComboSubject.ItemsSource = BDEntities.GetContext().Subjects.ToList();
            ComboGroup.ItemsSource = BDEntities.GetContext().Groups.ToList();

            using (var db = new BDEntities())
            {
                var students = db.Students.Include("Users").ToList();
                Students = new ObservableCollection<Students>(students);
            }

            DataContext = this;

            //Автоматическое добавление даты
            СomboYear.Items.Add(2019);

            // Add all years between 2019 and the current year to the combo box
            int currentYear = DateTime.Now.Year;
            for (int year = 2019; year <= currentYear; year++)
            {
                if (!СomboYear.Items.Contains(year))
                {
                    СomboYear.Items.Add(year);
                }
            }
            СomboYear.SelectedItem = currentYear;
        }
        private void ComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            int currentMonthIndex = DateTime.Now.Month - 1;
            ComboBox ComboMonth = sender as ComboBox;
            ComboMonth.SelectedIndex = currentMonthIndex;
        }

        private void Add_attedance_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddAttedance();
            window.ShowDialog();
            LoadGrid();
        }

        private void LoadGrid()
        {
            string groupName = null;
            if (ComboGroup.SelectedItem != null)
            {
                groupName = ((Groups)ComboGroup.SelectedItem).GroupName;
            }
            if (string.IsNullOrEmpty(groupName))
            {
                return;
            }

            int year = (int)СomboYear.SelectedItem;
            int month = ComboMonth.SelectedIndex + 1;
            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1).AddDays(-1);

            int subjectId = 0;
            if (ComboSubject.SelectedItem != null)
            {
                subjectId = ((Subjects)ComboSubject.SelectedItem).SubjectId;
            }

            using (var db = new BDEntities())
            {
                var query = from s in db.Students
                            join g in db.Groups on s.GroupId equals g.GroupId
                            join u in db.Users on s.UserId equals u.Id
                            join a in db.Attendance.Where(a => a.Date >= startDate && a.Date <= endDate && a.SubjectId == subjectId)
                                on s.Id equals a.StudentId into aGroup
                            from a in aGroup.DefaultIfEmpty()
                            join sub in db.Subjects on a.SubjectId equals sub.SubjectId into subGroup
                            from sub in subGroup.DefaultIfEmpty()
                            let passType = a != null ? a.PassType : false // добавляем переменную passType, которая будет содержать тип пропуска
                            where g.GroupName == groupName
                            orderby s.Id
                            select new { u.Name, StudentId = s.Id, Date = (a != null ? a.Date : default(DateTime?)), HasAbsence = (a != null), SubjectName = (sub != null ? sub.Name : ""), PassType = passType };


                var rstEdata = query.ToList();

                var employeeAttendanceList = new List<EmployeeAttendance>();
                var employeeAttendance = new EmployeeAttendance();
                var lastEmpID = -1;

                foreach (var row in rstEdata)
                {
                    var empID = row.StudentId;
                    var day = row.Date?.Day.ToString() ?? "";

                    if (empID != lastEmpID)
                    {
                        if (lastEmpID != -1)
                        {
                            employeeAttendanceList.Add(employeeAttendance);
                        }
                        employeeAttendance = new EmployeeAttendance { Name = row.Name }; // заменяем StudentId на Name
                        lastEmpID = empID;
                    }

                    var passType = row.HasAbsence ? (row.PassType ? "УП" : "H") : "";
                    var propertyDescriptor = TypeDescriptor.GetProperties(typeof(EmployeeAttendance))[$"Day{day}"];
                    propertyDescriptor?.SetValue(employeeAttendance, passType);
                }

                employeeAttendanceList.Add(employeeAttendance);

                foreach (var attendanceCount in employeeAttendanceList)
                {
                    var propertyDescriptorH = TypeDescriptor.GetProperties(typeof(EmployeeAttendance))["UnexcusedAbsences"];
                    propertyDescriptorH?.SetValue(attendanceCount, attendanceCount.UnexcusedAbsences);
                    var propertyDescriptorUP = TypeDescriptor.GetProperties(typeof(EmployeeAttendance))["UnAbsences"];
                    propertyDescriptorUP?.SetValue(attendanceCount, attendanceCount.UnAbsences);
                }

                myDataGrid.ItemsSource = employeeAttendanceList;

            }
        }


        private void ComboGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGrid();
        }


        private void CreateTable()
        {
            for (int i = 1; i <= 31; i++)
            {
                myDataGrid.Columns.Add(new DataGridTextColumn
                {
                    Header = i.ToString(),
                    Binding = new Binding($"Day{i}"),
                    IsReadOnly = true
                });
            }
            myDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Отсутствие\nпо неуважительной\nпричине",
                Binding = new Binding("UnexcusedAbsences"),
                IsReadOnly = true
            });
            myDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Отсутствие\nпо уважительной\nпричине",
                Binding = new Binding("UnAbsences"),
                IsReadOnly = true
            });

        }

        private void ComboMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGrid();
        }

        private void ComboYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGrid();
        }

        private void ComboSubject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadGrid();
        }

        private void Graphs_Click(object sender, RoutedEventArgs e)
        {
            if (ComboGroup.SelectedItem != null || ComboSubject.SelectedItem!=null)
            {
                var report = new Report(((Groups)ComboGroup.SelectedItem).GroupName, ((Subjects)ComboSubject.SelectedItem).Name, (int)СomboYear.SelectedItem, (int)ComboMonth.SelectedIndex);
                report.Show();
                LoadGrid();
            }
            else
                MessageBox.Show("Выберите группу и предмет!");
        }

        private void ExportToWord_Click(object sender, RoutedEventArgs e)
        {
            int selectedMonth = ComboMonth.SelectedIndex + 1;
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string folderName = "Посещаемость-Отчет";
            string folderPath = Path.Combine(desktopPath, folderName);
            Directory.CreateDirectory(folderPath);

            if (ComboGroup.SelectedItem != null || ComboSubject.SelectedItem != null)
            {
                try
                {
                    using (var db = new BDEntities())
                    {
                        string groupName = ((Groups)ComboGroup.SelectedItem).GroupName;
                        var queryInStudents = from s in db.Students
                                              join g in db.Groups on s.GroupId equals g.GroupId
                                              join u in db.Users on s.UserId equals u.Id
                                              select new { u.Name, g.GroupName, s.Id };
                        var students = queryInStudents.Where(s => s.GroupName == groupName).ToList();
                        var subject = ComboSubject.SelectedItem as Subjects;
                        var queryInAttendence = from a in db.Attendance
                                                join s in db.Students on a.StudentId equals s.Id
                                                join g in db.Groups on s.GroupId equals g.GroupId
                                                select new { AttendanceId = a.Id, g.GroupName, s.Id, a.SubjectId, a.Date, a.PassType };
                        var attendance = queryInAttendence.Where(s => s.GroupName == groupName && s.SubjectId == subject.SubjectId && s.Date.Month == selectedMonth).ToList();

                        //Создаем документ Word
                        string fileName = $"{subject.Name} - {groupName} - {ComboMonth.SelectedItem} {СomboYear.SelectedItem}.docx".Replace('/', '-');
                        string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                        string cleanedFileName = new string(fileName.Where(x => !invalidChars.Contains(x)).ToArray());
                        string path = Path.Combine(folderPath, cleanedFileName);
                        using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document))
                        {
                            
                            //Создаем главный раздел документа
                            MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();

                            //Добавляем стили в документ
                            StyleDefinitionsPart styleDefinitionsPart = mainPart.AddNewPart<StyleDefinitionsPart>();
                            styleDefinitionsPart.Styles = new Styles();
                            styleDefinitionsPart.Styles.Save();

                            //Создаем документ и добавляем заголовок
                            Document doc = new Document(); 
                            Body body = new Body();
                            mainPart.Document.Append(body);
                            Paragraph paraTitle = new Paragraph(new Run(new Text(cleanedFileName.Replace(".docx", ""))));
                            paraTitle.ParagraphProperties = new ParagraphProperties(
                                new Justification() { Val = JustificationValues.Center });
                            body.Append(paraTitle);

                            //Добавляем таблицу
                            Table table = new Table();
                            TableProperties tblProp = new TableProperties();
                            TableWidth tblWidth = new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct };
                            tblProp.Append(tblWidth);

                            //Add borders
                            TableBorders borders = new TableBorders();
                            borders.TopBorder = new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 };
                            borders.BottomBorder = new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 };
                            borders.LeftBorder = new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 };
                            borders.RightBorder = new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 };
                            borders.InsideHorizontalBorder = new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 };
                            borders.InsideVerticalBorder = new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 };
                            tblProp.Append(borders);

                            table.AppendChild(tblProp);

                            //Add rows and cells
                            TableRow tr = new TableRow();
                            TableCell th = new TableCell(new Paragraph(new Run(new Text("ФИО"))));
                            tr.Append(th);

                            //Create table cells for all days in the selected month
                            for (int day = 1; day <= DateTime.DaysInMonth(DateTime.Now.Year, selectedMonth); day++)
                            {
                                TableCell cell = new TableCell(new Paragraph(new Run(new Text(day.ToString()))));
                                tr.Append(cell);
                            }

                            table.Append(tr);

                            foreach (var student in students)
                            {
                                TableRow trStudent = new TableRow();
                                TableCell tdName = new TableCell(new Paragraph(new Run(new Text(student.Name))));
                                trStudent.Append(tdName);

                                //Create table cells for all days in the selected month
                                for (int day = 1; day <= DateTime.DaysInMonth(DateTime.Now.Year, selectedMonth); day++)
                                {
                                    var att = attendance.FirstOrDefault(x => x.Date.Day == day && x.Id == student.Id);
                                    string attString = att != null ? (att.PassType ? "②" : "2") : "";
                                    TableCell tdAttendance = new TableCell(new Paragraph(new Run(new Text(attString))));
                                    trStudent.Append(tdAttendance);
                                }

                                table.Append(trStudent);
                            }
                            body.Append(table);
                            doc.Append(body);
                            mainPart.Document = doc;
                            mainPart.Document.Save();
                            wordDoc.Close();

                            MessageBox.Show("Файл успешно сохранен.");

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте в Word: {ex.Message}");
                }
            }
            else
                MessageBox.Show("Выберите группу и предмет!");

        }
    }
}