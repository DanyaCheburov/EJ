using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EJ.AttendanceManagement;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;


namespace EJ.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для JournalPage.xaml
    /// </summary>
    public partial class JournalPage : Page
    {
        public ObservableCollection<Students> Students { get; set; }
        public Subjects SelectedSubject { get; set; }
        public int SelectedMonthIndex { get; set; }

        public JournalPage()
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

            СomboYear.Items.Add(2019);
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
                            join j in db.Journal.Where(j => j.Date >= startDate && j.Date <= endDate && j.SubjectId == subjectId)
                                on s.StudentId equals j.StudentId into aGroup
                            from a in aGroup.DefaultIfEmpty()
                            join sub in db.Subjects on a.SubjectId equals sub.SubjectId into subGroup
                            from sub in subGroup.DefaultIfEmpty()
                            where g.GroupName == groupName
                            orderby s.StudentId
                            select new { u.UserName, s.StudentId, Date = (a != null ? a.Date : default(DateTime?)), HasAbsence = (a != null), SubjectName = (sub != null ? sub.SubjectName : ""), Score = (a != null ? a.Estimate : default) };

                var rstEdata = query.ToList();

                var employeeJournalList = new List<EmployeeJournal>();
                var employeeJournal = new EmployeeJournal();
                var lastEmpID = -1;

                foreach (var row in rstEdata)
                {

                    var empID = row.StudentId;
                    var day = row.Date?.Day.ToString() ?? "";

                    if (empID != lastEmpID)
                    {
                        if (lastEmpID != -1)
                        {
                            employeeJournalList.Add(employeeJournal);
                        }
                        employeeJournal = new EmployeeJournal { Name = row.UserName };
                        lastEmpID = empID;
                    }
                    var score = row.Score.ToString() ?? "";
                    var propertyDescriptor = TypeDescriptor.GetProperties(typeof(EmployeeJournal))[$"Day{day}"];
                    propertyDescriptor?.SetValue(employeeJournal, score);

                }

                employeeJournalList.Add(employeeJournal);

                myDataGrid.ItemsSource = employeeJournalList;

            }
        }
        private void LoadThemeGrid()
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

            int subjectId = SelectedSubject?.SubjectId ?? 0;
            if (ComboSubject.SelectedItem != null)
            {
                subjectId = ((Subjects)ComboSubject.SelectedItem).SubjectId;
            }

            using (var db = new BDEntities())
            {
                // Выполнение запроса LINQ to Entities без операции .Date
                var query = from l in db.Lesson_themes
                            join g in db.Groups on l.Group_id equals g.GroupId
                            join t in db.Lesson_themes.Where(j => j.Date >= startDate && j.Date <= endDate && j.Subject_id == subjectId)
                                on l.Subject_id equals t.Subject_id into aGroup
                            from a in aGroup.DefaultIfEmpty()
                            join sub in db.Subjects on a.Subject_id equals sub.SubjectId into subGroup
                            from sub in subGroup.DefaultIfEmpty()
                            where g.GroupName == groupName
                            select new { Date = (a != null ? a.Date : default(DateTime?)), Description = (a != null ? a.Description : "") };

                // Извлечение данных из базы данных
                var rstEdata = query.ToList();

                // Преобразование даты на стороне клиента
                var processedData = rstEdata
                    .Select(x => new { Date = x.Date?.Date.ToString("yyyy-MM-dd"), x.Description })
                    .Distinct()
                    .ToList();

                // Назначение обработанных данных на DataGrid
                ThemeDataGrid.ItemsSource = processedData;

            }
        }


        private void ComboGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (Theme.IsChecked == true)
            {
                LoadThemeGrid();
                myDataGrid.Visibility = Visibility.Hidden;
                ThemeDataGrid.Visibility = Visibility.Visible;
            }
            else
            {
                LoadGrid();
                myDataGrid.Visibility = Visibility.Visible;
                ThemeDataGrid.Visibility = Visibility.Hidden;
            }
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
            ThemeDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Тема",
                Binding = new Binding("Description"),
                IsReadOnly = true
            });
            ThemeDataGrid.Columns.Add(new DataGridTextColumn
            {
                Header = "Date",
                Binding = new Binding("Date"),
                IsReadOnly = true
            });


        }
        private void ComboMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Theme.IsChecked == true)
            {
                LoadThemeGrid();
                myDataGrid.Visibility = Visibility.Hidden;
                ThemeDataGrid.Visibility = Visibility.Visible;
            }
            else
            {
                LoadGrid();
                myDataGrid.Visibility = Visibility.Visible;
                ThemeDataGrid.Visibility = Visibility.Hidden;
            }
        }

        private void ComboYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Theme.IsChecked == true)
            {
                LoadThemeGrid();
                myDataGrid.Visibility = Visibility.Hidden;
                ThemeDataGrid.Visibility = Visibility.Visible;
            }
            else
            {
                LoadGrid();
                myDataGrid.Visibility = Visibility.Visible;
                ThemeDataGrid.Visibility = Visibility.Hidden;
            }
        }

        private void ComboSubject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Theme.IsChecked == true)
            {
                LoadThemeGrid();
                myDataGrid.Visibility = Visibility.Hidden;
                ThemeDataGrid.Visibility = Visibility.Visible;
            }
            else
            {
                LoadGrid();
                myDataGrid.Visibility = Visibility.Visible;
                ThemeDataGrid.Visibility = Visibility.Hidden;
            }
        }

        private void Add_estimate_Click(object sender, RoutedEventArgs e)
        {

            if (Theme.IsChecked == true)
            {
                Add_estimate.Content = "Добавить тему";
               // var window = new AddTheme();
              //  window.ShowDialog
                   
                LoadThemeGrid();
            }
            else
            {
                var window = new AddEstimate();
                window.ShowDialog();
                LoadGrid();
            }
        }
        private void Theme_Click(object sender, RoutedEventArgs e)
        {
            if (Theme.IsChecked == true)
            {
                LoadThemeGrid();
                myDataGrid.Visibility = Visibility.Hidden;
                ThemeDataGrid.Visibility = Visibility.Visible;
            }
            else
            {
                LoadGrid();
                myDataGrid.Visibility = Visibility.Visible;
                ThemeDataGrid.Visibility = Visibility.Hidden;
            }
        }

        private void Graphs_Click(object sender, RoutedEventArgs e)
        {
            if (ComboGroup.SelectedItem != null && ComboSubject.SelectedItem != null)
            {
                var report = new ProgressReport(((Groups)ComboGroup.SelectedItem).GroupName, ((Subjects)ComboSubject.SelectedItem).SubjectName, (int)СomboYear.SelectedItem, (int)ComboMonth.SelectedIndex);
                report.Show();
            }
            else
                MessageBox.Show("Выберите группу и предмет!");
        }

        private double CalculateAverageScore(string userName, string subjectName, List<DateTime> dates, List<EstimateEntry> estimates)
        {
            var studentEstimates = estimates.Where(j => j.UserName == userName && j.SubjectName == subjectName).ToList();
            int totalEstimates = studentEstimates.Count;
            int totalScore = studentEstimates.Sum(j => j.Estimate);

            double averageScore = totalEstimates > 0 ? (double)totalScore / totalEstimates : 0.0;

            return averageScore;
        }

        private void ExportToWord_CLick(object sender, RoutedEventArgs e)
        {
            string selectedMonthText = ((ComboBoxItem)ComboMonth.SelectedItem).Content.ToString();

            int selectedMonth = ComboMonth.SelectedIndex + 1;
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string folderName = "Успеваемость-Отчет";
            string folderPath = Path.Combine(desktopPath, folderName);
            Directory.CreateDirectory(folderPath);

            if (ComboGroup.SelectedItem != null && ComboSubject.SelectedItem != null)
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
                        var queryInJournal = from j in db.Journal
                                             join sb in db.Subjects on j.SubjectId equals sb.SubjectId
                                             join st in db.Students on j.StudentId equals st.StudentId
                                             join u in db.Users on st.UserId equals u.UserId
                                             join g in db.Groups on st.GroupId equals g.GroupId
                                             join lt in db.Lesson_themes on sb.SubjectId equals lt.Subject_id
                                             select new EstimateEntry
                                             {
                                                 UserName = u.UserName,
                                                 GroupName = g.GroupName,
                                                 SubjectName = sb.SubjectName,
                                                 Date = j.Date,
                                                 Estimate = j.Estimate,
                                                 LessonDate = lt.Date,
                                                 Description = lt.Description
                                             };

                        var estimates = queryInJournal.Where(j => j.GroupName == groupName && j.SubjectName == subject.SubjectName && j.Date.Month == selectedMonth).ToList();
                        var dates = estimates.Select(j => j.Date.Date).Distinct().ToList();

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
                            Paragraph paraTitle = new Paragraph(new Run(new Text(cleanedFileName.Replace(".docx", ""))))
                            {
                                ParagraphProperties = new ParagraphProperties(
                                new Justification() { Val = JustificationValues.Center })
                            };
                            body.Append(paraTitle);

                            //Добавляем таблицу
                            Table table = new Table();
                            TableProperties tblProp = new TableProperties(new TableWidth() { Width = "100%", Type = TableWidthUnitValues.Pct },
                                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center });
                            tblProp.Append();

                            TableBorders borders = new TableBorders
                            {
                                TopBorder = new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                BottomBorder = new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                LeftBorder = new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                RightBorder = new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                InsideHorizontalBorder = new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                InsideVerticalBorder = new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                            };
                            tblProp.Append(borders);

                            table.AppendChild(tblProp);

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

                            foreach (var day in dates)
                            {
                                TableCell cell = new TableCell(new Paragraph(new Run(new Text(day.ToString("d.MM")))));
                                TableCellProperties cellProps = new TableCellProperties(
                                    new TableCellWidth() { Width = "2%" },
                                    new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center });
                                cell.Append(cellProps);
                                tr.Append(cell);
                            }
                            TableCell thAverage = new TableCell(new Paragraph(new Run(new Text("Средний балл"))));
                            thAverage.Append(new TableCellProperties(
                                new TableCellWidth() { Width = "10%" },
                                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }));
                            tr.Append(thAverage);

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

                                foreach (var day in dates)
                                {
                                    var estimate = estimates.FirstOrDefault(x => x.Date.Date == day && x.UserName == student.UserName);
                                    string estimateString = estimate != null ? estimate.Estimate.ToString() : "";
                                    TableCell tdEstimate = new TableCell(new Paragraph(new Run(new Text(estimateString))));
                                    trStudent.Append(tdEstimate);
                                }

                                // Добавляем ячейку для среднего балла
                                double averageScore = CalculateAverageScore(student.UserName, subject.SubjectName, dates, estimates);
                                string averageScoreString = averageScore != 0.0 ? averageScore.ToString() : "-";
                                TableCell tdAverageScore = new TableCell(new Paragraph(new Run(new Text(averageScoreString))));
                                trStudent.Append(tdAverageScore);

                                table.Append(trStudent);
                            }

                            int rowIndex = 0;
                            foreach (TableRow row in table.Elements<TableRow>())
                            {
                                foreach (TableCell cell in row.Elements<TableCell>())
                                {
                                    if (rowIndex == 0) // Проверяем, что это заголовок таблицы
                                    {
                                        foreach (Paragraph paragraph in cell.Elements<Paragraph>())
                                        {
                                            foreach (Run run in paragraph.Elements<Run>())
                                            {
                                                foreach (Text text in run.Elements<Text>())
                                                {
                                                    RunProperties runProperties = new RunProperties();
                                                    runProperties.Append(new Bold());

                                                    run.RunProperties = runProperties;
                                                }
                                            }
                                        }
                                    }
                                    else // Для остальных ячеек таблицы не применяем жирное начертание
                                    {
                                        TableCellProperties cellProperties = new TableCellProperties();
                                        cellProperties.Append(new TableCellWidth() { Type = TableWidthUnitValues.Auto });
                                        cellProperties.Append(new TableCellBorders(
                                            new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 2 },
                                            new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 2 },
                                            new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 2 },
                                            new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 2 }
                                        ));

                                        cell.Append(cellProperties);
                                    }
                                }
                                rowIndex++;
                            }
                            // Добавляем таблицу в тело документа
                            body.Append(table);
                            body.Append(orientation);
                            doc.Append(body);
                            mainPart.Document = doc;
                            mainPart.Document.Save();
                            wordDoc.Dispose(); 
                        }
                        using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(path, true))
                        {
                            // Находим последний раздел документа
                            MainDocumentPart mainPart = wordDoc.MainDocumentPart;
                            SectionProperties sectionProperties = mainPart.Document.Body.Elements<SectionProperties>().LastOrDefault();
                            if (sectionProperties == null)
                            {
                                // Если раздел не существует, создаем новый раздел
                                sectionProperties = new SectionProperties();
                                mainPart.Document.Body.Append(sectionProperties);
                            }

                            // Создаем раздел со страницей
                            SectionProperties newSectionProperties = new SectionProperties(new PageSize()
                            {
                                Width = (UInt32Value)15840U,
                                Height = (UInt32Value)12240U,
                                Orient = PageOrientationValues.Landscape
                            },
                            new PageMargin());
                            mainPart.Document.Body.Append(newSectionProperties);

                            // Создаем заголовок
                            Paragraph paraTitle = new Paragraph(new Run(new Text("Темы занятий")))
                            {
                                ParagraphProperties = new ParagraphProperties(
                                new Justification() { Val = JustificationValues.Center })
                            };
                            mainPart.Document.Body.Append(paraTitle);

                            // Добавляем таблицу
                            Table table = new Table();
                            TableProperties tblProp = new TableProperties(new TableWidth() { Width = "100%", Type = TableWidthUnitValues.Pct },
                                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center });
                            tblProp.Append();

                            TableBorders borders = new TableBorders
                            {
                                TopBorder = new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                BottomBorder = new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                LeftBorder = new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                RightBorder = new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                InsideHorizontalBorder = new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 },
                                InsideVerticalBorder = new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 4 }
                            };
                            tblProp.Append(borders);

                            table.AppendChild(tblProp);

                            // Добавляем заголовки столбцов
                            TableRow headerRow = new TableRow();

                            // Добавляем заголовок для даты темы занятия
                            TableCell dateCell = new TableCell(new Paragraph(new Run(new Text("Дата темы занятия"))));
                            dateCell.Append(new TableCellProperties(
                                new TableCellWidth() { Width = "10%" },
                                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }));
                            headerRow.Append(dateCell);

                            // Добавляем заголовок для темы занятия
                            TableCell themeCell = new TableCell(new Paragraph(new Run(new Text("Тема занятия"))));
                            themeCell.Append(new TableCellProperties(
                                new TableCellWidth() { Width = "90%" },
                                new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }));
                            headerRow.Append(themeCell);

                            table.Append(headerRow);

                            // Добавляем данные в таблицу
                            foreach (var item in dates)
                            {
                                TableRow dataRow = new TableRow();

                                // Добавляем ячейку для даты темы занятия
                                TableCell dateCellData = new TableCell(new Paragraph(new Run(new Text(item.Date.ToString("d.MM")))));
                                dateCellData.Append(new TableCellProperties(
                                    new TableCellWidth() { Width = "10%" },
                                    new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }));
                                dataRow.Append(dateCellData);

                                var lessonTheme = db.Lesson_themes.FirstOrDefault(lt => lt.Date == item.Date);
                                string theme = lessonTheme != null ? lessonTheme.Description : "";
                                TableCell themeCellData = new TableCell(new Paragraph(new Run(new Text(theme))));
                                themeCellData.Append(new TableCellProperties(
                                    new TableCellWidth() { Width = "90%" },
                                    new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center }));
                                dataRow.Append(themeCellData);

                                table.Append(dataRow);
                            }
                            // Добавляем таблицу в тело документа
                            mainPart.Document.Body.Append(table);

                            mainPart.Document.Save();
                            wordDoc.Dispose();
                        }
                    }
                    MessageBox.Show("Отчет успешно сохранен на рабочем столе", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте в Word: {ex.Message}");
                }
            }
            else MessageBox.Show("Выберите группу или предмет");
        }
    }
}