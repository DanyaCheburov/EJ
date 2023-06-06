using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EJ.MainMenu
{
    /// <summary>
    /// Логика взаимодействия для AttendanceStudentPage.xaml
    /// </summary>
    public partial class AttendanceStudentPage : Page
    {
        private readonly BDEntities db = new BDEntities();
        public AttendanceStudentPage()
        {
            InitializeComponent();
            SetUserInfo();
            SetCurrentMonthDates();
        }

        private void SetCurrentMonthDates()
        {
            DateTime startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            StartOfPeriod.SelectedDate = startOfMonth;

            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            EndOfPeriod.SelectedDate = endOfMonth;
        }
        private void SetUserInfo()
        {
            string userName = Application.Current.Properties["Name"] as string;
            NameTextBlock.Text = userName;

            int currentUser = (int)Application.Current.Properties["UserId"];

            using (var dbContext = new BDEntities())
            {
                var student = dbContext.Students.FirstOrDefault(s => s.UserId == currentUser);
                if (student != null)
                {
                    var groupId = student.GroupId;
                    var group = dbContext.Groups.FirstOrDefault(g => g.GroupId == groupId);
                    if (group != null)
                    {
                        string groupName = group.GroupName;
                        GroupsTextBlock.Text = groupName;
                    }
                }
            }
        }
        private int GetStudentId()
        {
            int currentUser = (int)Application.Current.Properties["UserId"];

            using (var dbContext = new BDEntities())
            {
                var student = dbContext.Students.FirstOrDefault(s => s.UserId == currentUser);
                if (student != null)
                {
                    return student.StudentId;
                }
            }
            return -1; // Если не удалось получить идентификатор студента, возвращаем значение по умолчанию
        }
        private List<DateTime> GenerateUniqueDates(DateTime startDate, DateTime endDate, int studentId)
        {
            var query = from attendance in db.Attendance
                        join student in db.Students on attendance.StudentId equals student.StudentId
                        join subject in db.Subjects on attendance.SubjectId equals subject.SubjectId
                        where student.StudentId == studentId &&
                              attendance.Date >= startDate && attendance.Date <= endDate
                        select new
                        {
                            attendance.Date
                        };

            var reportData = query.ToList();
            var uniqueDates = reportData.Select(item => item.Date.Date).Distinct().OrderBy(date => date).ToList();

            return uniqueDates;
        }

        private List<AttendanceReportItem> GenerateReportData(DateTime startDate, DateTime endDate, int studentId, List<DateTime> uniqueDates)
        {
            var query = from attendance in db.Attendance
                        join student in db.Students on attendance.StudentId equals student.StudentId
                        join subject in db.Subjects on attendance.SubjectId equals subject.SubjectId
                        where student.StudentId == studentId &&
                              attendance.Date >= startDate && attendance.Date <= endDate
                        select new
                        {
                            subject.SubjectName,
                            attendance.Date,
                            attendance.PassType
                        };

            var reportData = query.ToList();
            List<AttendanceReportItem> reportItems = new List<AttendanceReportItem>();

            foreach (var item in reportData)
            {
                string passType = item.PassType ? "УП" : "НП";
                AttendanceReportItem reportItem = reportItems.FirstOrDefault(r => r.SubjectName == item.SubjectName);

                if (reportItem == null)
                {
                    reportItem = new AttendanceReportItem
                    {
                        SubjectName = item.SubjectName
                    };
                    reportItems.Add(reportItem);
                }

                if (!reportItem.DateData.ContainsKey(item.Date.Date))
                {
                    reportItem.DateData.Add(item.Date.Date, passType);

                    if (passType == "УП")
                    {
                        reportItem.UPCount += 2;
                    }
                    else if (passType == "НП")
                    {
                        reportItem.NCount += 2;
                    }
                }
            }

            return reportItems;
        }
        private bool isToCreateDone = false;
        private void ToCreate_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate = StartOfPeriod.SelectedDate ?? DateTime.MinValue;
            DateTime endDate = EndOfPeriod.SelectedDate ?? DateTime.MaxValue;
            int studentId = GetStudentId();

            List<DateTime> uniqueDates = GenerateUniqueDates(startDate, endDate, studentId);
            List<AttendanceReportItem> reportItems = GenerateReportData(startDate, endDate, studentId, uniqueDates);

            // Очистка существующих столбцов в DataGrid
            myDataGrid.Columns.Clear();

            // Добавление столбца для предметов в DataGrid
            DataGridTextColumn subjectColumn = new DataGridTextColumn
            {
                Header = "Предмет",
                Binding = new Binding("SubjectName"),
                IsReadOnly = true
            };
            myDataGrid.Columns.Add(subjectColumn);

            // Добавление столбцов для дат в DataGrid в отсортированном порядке
            foreach (var date in uniqueDates)
            {
                DataGridTextColumn dateColumn = new DataGridTextColumn
                {
                    Header = date.ToString("dd.MM.yy"),
                    Binding = new Binding(string.Format("DateData[{0:yyyy-MM-dd}]", date.Date)),
                    IsReadOnly = true
                };
                myDataGrid.Columns.Add(dateColumn);
            }

            // Добавление столбца "Отсутствие (считает всего УП) по уважительной причине"
            DataGridTextColumn upCountColumn = new DataGridTextColumn
            {
                Header = "Отсутствие\nпо уважительной\nпричине",
                Binding = new Binding("UPCount"),
                IsReadOnly = true
            };
            myDataGrid.Columns.Add(upCountColumn);

            // Добавление столбца "Отсутствие (считает всего Н) по неуважительной причине"
            DataGridTextColumn nCountColumn = new DataGridTextColumn
            {
                Header = "Отсутствие\nпо неуважительной\nпричине",
                Binding = new Binding("NCount"),
                IsReadOnly = true
            };
            myDataGrid.Columns.Add(nCountColumn);

            // Обновление данных в DataGrid
            myDataGrid.ItemsSource = reportItems;
            myDataGrid.Visibility = Visibility.Visible;
            isToCreateDone = true;
        }

        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            if (!isToCreateDone)
            {
                MessageBox.Show("Необходимо выполнить 'Сформировать отчет' перед 'Экспорт в PDF'");
                return;
            }
            DateTime startDate = StartOfPeriod.SelectedDate ?? DateTime.MinValue;
            DateTime endDate = EndOfPeriod.SelectedDate ?? DateTime.MaxValue;

            int studentId = GetStudentId();

            List<DateTime> uniqueDates = GenerateUniqueDates(startDate, endDate, studentId);
            List<AttendanceReportItem> reportItems = GenerateReportData(startDate, endDate, studentId, uniqueDates);

            // Создание документа PDF
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF файлы (*.pdf)|*.pdf"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                // Создание базового шрифта для русского языка
                BaseFont russianBaseFont = BaseFont.CreateFont("c:/windows/fonts/arial.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                Document document = new Document();
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(saveFileDialog.FileName, FileMode.Create));
                document.Open();

                // Шрифт для дат, предметов, пропусков
                Font FontInfo = new Font(russianBaseFont, 9, Font.NORMAL);
                //Шрифт для столбцов с отсуствием
                Font FontColumn = new Font(russianBaseFont, 8, Font.NORMAL);
                //Шрифт для заголовка
                Font FontHeading = new Font(russianBaseFont, 20f, Font.BOLD);
                //Шрифт для информации о студенте
                Font FontInfoUser = new Font(russianBaseFont, 15, Font.NORMAL);

                string studentName = NameTextBlock.Text;
                string groupName = GroupsTextBlock.Text;
                string period = startDate.ToString("dd.MM.yy") + " - " + endDate.ToString("dd.MM.yy");

                // Добавление информации о студенте, группе, пермода
                Paragraph studentParagraph = new Paragraph("Студент: " + studentName, FontInfoUser)
                {
                    Alignment = Element.ALIGN_LEFT
                };
                document.Add(studentParagraph);

                Paragraph groupParagraph = new Paragraph("Группа: " + groupName, FontInfoUser)
                {
                    Alignment = Element.ALIGN_LEFT
                };
                document.Add(groupParagraph);

                Paragraph periodParagraph = new Paragraph("Период: " + period, FontInfoUser)
                {
                    Alignment = Element.ALIGN_LEFT
                };
                document.Add(periodParagraph);

                document.Add(new Paragraph(" ")); // Пустой абзац

                // Добавление заголовка
                Paragraph title = new Paragraph("Отчет об посещаемости студента", FontHeading)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(title);

                // Добавление отступа
                Paragraph emptyParagraph = new Paragraph(" "); // Пустой абзац
                document.Add(emptyParagraph);

                // Добавление данных таблицы
                PdfPTable table = new PdfPTable(myDataGrid.Columns.Count)
                {
                    WidthPercentage = 100
                };

                // Добавление столбца "Предмет"
                PdfPCell subjectCell = new PdfPCell(new Phrase("Предмет", FontInfo))
                {
                    BackgroundColor = new BaseColor(230, 230, 230)
                };
                table.AddCell(subjectCell);

                foreach (var date in uniqueDates)
                {
                    PdfPCell dateCell = new PdfPCell(new Phrase(date.ToString("dd.MM.yy"), FontInfo))
                    {
                        HorizontalAlignment = Element.ALIGN_CENTER // Выравнивание содержимого по центру
                    };
                    table.AddCell(dateCell);
                }

                // Добавление столбца "Отсутствие по уважительной причине"
                PdfPCell upCountCell = new PdfPCell(new Phrase("Отсутствие\nпо уважительной\nпричине", FontColumn))
                {
                    BackgroundColor = new BaseColor(230, 230, 230)
                };
                table.AddCell(upCountCell);

                // Добавление столбца "Отсутствие по неуважительной причине"
                PdfPCell nCountCell = new PdfPCell(new Phrase("Отсутствие\nпо неуважительной\nпричине", FontColumn))
                {
                    BackgroundColor = new BaseColor(230, 230, 230)
                };
                table.AddCell(nCountCell);


                PdfPCell upCountCell1; // Объявление переменной upCountCell перед циклом
                PdfPCell nCountCell1; // Объявление переменной nCountCell перед циклом

                // Добавление строк и данных таблицы
                foreach (AttendanceReportItem reportItem in reportItems)
                {
                    PdfPCell subjectName = new PdfPCell(new Phrase(reportItem.SubjectName, FontInfo));
                    table.AddCell(subjectName);

                    foreach (var date in uniqueDates)
                    {
                        string passType = reportItem.DateData.ContainsKey(date) ? reportItem.DateData[date] : "";
                        PdfPCell passTypePDF = new PdfPCell(new Phrase(passType, FontInfo))
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER // Центрирование содержимого
                        };
                        table.AddCell(passTypePDF); // Добавление ячейки с типом пропуска для каждой даты
                    }

                    // Присваивание значений переменным upCountCell и nCountCell
                    upCountCell1 = new PdfPCell(new Phrase(reportItem.UPCount.ToString(), FontInfo));
                    nCountCell1 = new PdfPCell(new Phrase(reportItem.NCount.ToString(), FontInfo));

                    // Добавление ячеек с количеством уважительных и неуважительных пропусков
                    table.AddCell(upCountCell1);
                    table.AddCell(nCountCell1);
                }

                document.Add(table);

                // Закрытие документа
                document.Close();
                MessageBox.Show("Отчет успешно создан!");
            }
        }
    }
}
