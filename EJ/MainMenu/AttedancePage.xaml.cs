using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EJ.AttendanceManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


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

        private void PassManagement_Click(object sender, RoutedEventArgs e)
        {
            var window = new PassManagement();
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
                            join u in db.Users on s.UserId equals u.UserId
                            join a in db.Attendance.Where(a => a.Date >= startDate && a.Date <= endDate && a.SubjectId == subjectId)
                                on s.StudentId equals a.StudentId into aGroup
                            from a in aGroup.DefaultIfEmpty()
                            join sub in db.Subjects on a.SubjectId equals sub.SubjectId into subGroup
                            from sub in subGroup.DefaultIfEmpty()
                            let passType = a != null ? a.PassType : false // добавляем переменную passType, которая будет содержать тип пропуска
                            where g.GroupName == groupName
                            orderby s.StudentId
                            select new { u.UserName, StudentId = s.StudentId, Date = (a != null ? a.Date : default(DateTime?)), HasAbsence = (a != null), SubjectName = (sub != null ? sub.SubjectName : ""), PassType = passType };


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
                        employeeAttendance = new EmployeeAttendance { Name = row.UserName }; // заменяем StudentId на Name
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
            if (ComboGroup.SelectedItem != null || ComboSubject.SelectedItem != null)
            {
                var report = new Report(((Groups)ComboGroup.SelectedItem).GroupName, ((Subjects)ComboSubject.SelectedItem).SubjectName, (int)СomboYear.SelectedItem, (int)ComboMonth.SelectedIndex);
                report.Show();
                LoadGrid();
            }
            else
                MessageBox.Show("Выберите группу и предмет!");
        }

        private void ExportToWord_Click(object sender, RoutedEventArgs e)
        {
            string selectedMonthText = ((ComboBoxItem)ComboMonth.SelectedItem).Content.ToString();

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
                                              join u in db.Users on s.UserId equals u.UserId
                                              select new { u.UserName, g.GroupName, s.StudentId };
                        var students = queryInStudents.Where(s => s.GroupName == groupName).ToList();
                        var subject = ComboSubject.SelectedItem as Subjects;
                        var queryInAttendence = from a in db.Attendance
                                                join s in db.Students on a.StudentId equals s.StudentId
                                                join g in db.Groups on s.GroupId equals g.GroupId
                                                select new { AttendanceId = a.AttendanceId, g.GroupName, s.StudentId, a.SubjectId, a.Date, a.PassType };
                        var attendance = queryInAttendence.Where(s => s.GroupName == groupName && s.SubjectId == subject.SubjectId && s.Date.Month == selectedMonth).ToList();

                        //Создаем документ Word
                        string fileName = $"{subject.SubjectName} - {groupName} - {selectedMonthText} {СomboYear.SelectedItem}.docx".Replace('/', '-');
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
                            Paragraph orientation = new Paragraph(new ParagraphProperties(new SectionProperties(new PageSize() 
                            { 
                                Width = (UInt32Value)15840U, 
                                Height = (UInt32Value)12240U, 
                                Orient = PageOrientationValues.Landscape 
                            },
                            new PageMargin())));
                            Paragraph paraTitle = new Paragraph(new Run(new Text(cleanedFileName.Replace(".docx", ""))));
                            paraTitle.ParagraphProperties = new ParagraphProperties(
                                new Justification() { Val = JustificationValues.Center });
                            body.Append(paraTitle);

                            //Добавляем таблицу
                            Table table = new Table();
                            TableProperties tblProp = new TableProperties(new TableWidth() { Width = "100%", Type = TableWidthUnitValues.Pct },
                                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center });                            
                            tblProp.Append();                            

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
                            TableCell th = new TableCell();
                            Paragraph p = new Paragraph(new Run(new Text("ФИО студента")));

                            TableCellProperties thProps = new TableCellProperties(
                                new TableCellWidth() { Width = "10%" },
                                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center });
                            th.Append(thProps);

                            // устанавливаем выравнивание по центру
                            p.ParagraphProperties = new ParagraphProperties(new Justification() { Val = JustificationValues.Center });
                            th.Append(p);
                            tr.Append(th);

                            // Get number of days in selected month
                            int numDaysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, selectedMonth);
                            
                            // Create table cells for all days in the selected month with the calculated width
                            for (int day = 1; day <= numDaysInMonth; day++)
                            {
                                TableCell cell = new TableCell(new Paragraph(new Run(new Text(day.ToString()))));
                                TableCellProperties cellProps = new TableCellProperties(
                                    new TableCellWidth() { Width = "2%" },
                                    new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center });
                                cell.Append(cellProps);
                                tr.Append(cell);
                            }
                            TableCell thValid = new TableCell(new Paragraph(new Run(new Text("По уважительной причиной"))));
                            TableCellProperties thValidProps = new TableCellProperties(new TableCellWidth() { Width = "3%" });
                            thValid.Append(thValidProps);
                            TableCell thInvalid = new TableCell(new Paragraph(new Run(new Text("Без уважительной причины"))));
                            TableCellProperties thInvalidProps = new TableCellProperties(new TableCellWidth() { Width = "3%" });
                            thInvalid.Append(thInvalidProps);
                            tr.Append(thValid);
                            tr.Append(thInvalid);
                            table.Append(tr);

                            foreach (var student in students)
                            {
                                TableRow trStudent = new TableRow();
                                string fullName = student.UserName;
                                string[] nameParts = fullName.Split(' ');
                                string fio;
                                if (nameParts.Length == 3)
                                {
                                    fio = $"{nameParts[0]} {nameParts[1][0]}.{nameParts[2][0]}.";
                                }
                                else
                                {
                                    fio = $"{nameParts[0]} {nameParts[1][0]}.";
                                }
                                TableCell tdName = new TableCell(new Paragraph(new Run(new Text(fio))));
                                trStudent.Append(tdName);

                                int validAbsences = attendance.Where(a => a.StudentId == student.StudentId && a.PassType == true).Count();
                                int invalidAbsences = attendance.Where(a => a.StudentId == student.StudentId && a.PassType == false).Count();

                                //Create table cells for all days in the selected month
                                for (int day = 1; day <= DateTime.DaysInMonth(DateTime.Now.Year, selectedMonth); day++)
                                {
                                    
                                    var att = attendance.FirstOrDefault(x => x.Date.Day == day && x.StudentId == student.StudentId);
                                    string attString = att != null ? (att.PassType ? "②" : "2") : "";
                                    TableCell tdAttendance = new TableCell(new Paragraph(new Run(new Text(attString))));
                                    trStudent.Append(tdAttendance);
                                }
                                table.Append(trStudent);
                                validAbsences *= 2;
                                invalidAbsences *= 2;

                                //Add cells for valid and invalid absences
                                TableCell tdValidAbsences = new TableCell(new Paragraph(new Run(new Text(validAbsences.ToString()))));
                                trStudent.Append(tdValidAbsences);

                                TableCell tdInvalidAbsences = new TableCell(new Paragraph(new Run(new Text(invalidAbsences.ToString()))));
                                trStudent.Append(tdInvalidAbsences);
                            }
                            foreach (TableRow row in table.Elements<TableRow>())
                            {
                                foreach (TableCell cell in row.Elements<TableCell>())
                                {
                                    ParagraphProperties props = new ParagraphProperties();
                                    Justification justification = new Justification() { Val = JustificationValues.Center };
                                    props.Append(justification);
                                    cell.Append(props);
                                }
                            }
                            body.Append(table);
                            body.Append(orientation);
                            
                            doc.Append(body);
                            mainPart.Document = doc;
                            
                            mainPart.Document.Save();
                            wordDoc.Dispose();

                            MessageBox.Show("Отчет успешно сохранен на рабочем столе", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

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
