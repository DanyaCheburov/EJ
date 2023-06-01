using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;

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
            string userName = Application.Current.Properties["Name"] as string;
            NameTextBlock.Text = userName;
            GetGroupName();
            SetCurrentMonthDates();
        }

        private void SetCurrentMonthDates()
        {
            // Установка начала текущего месяца
            DateTime startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            StartOfPeriod.SelectedDate = startOfMonth;

            // Установка конца текущего месяца
            DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            EndOfPeriod.SelectedDate = endOfMonth;
        }

        private void GetGroupName()
        {
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
        private void ToCreate_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate = StartOfPeriod.SelectedDate ?? DateTime.MinValue;
            DateTime endDate = EndOfPeriod.SelectedDate ?? DateTime.MaxValue;

            int studentId = GetStudentId();

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

            // Создание списка уникальных дат и сортировка их по возрастанию
            var uniqueDates = reportData.Select(item => item.Date.Date).Distinct().OrderBy(date => date).ToList();

            // Создание списка объектов AttendanceReportItem
            List<AttendanceReportItem> reportItems = new List<AttendanceReportItem>();

            foreach (var item in reportData)
            {
                string passType = item.PassType ? "УП" : "Н";

                // Поиск соответствующего объекта AttendanceReportItem для предмета
                AttendanceReportItem reportItem = reportItems.FirstOrDefault(r => r.SubjectName == item.SubjectName);

                if (reportItem == null)
                {
                    reportItem = new AttendanceReportItem
                    {
                        SubjectName = item.SubjectName
                    };
                    reportItems.Add(reportItem);
                }

                // Заполнение данных о пропусках для каждой даты
                if (!reportItem.DateData.ContainsKey(item.Date.Date))
                {
                    reportItem.DateData.Add(item.Date.Date, passType);

                    if (passType == "УП")
                    {
                        reportItem.UPCount += 2;
                    }
                    else if (passType == "Н")
                    {
                        reportItem.NCount += 2;
                    }
                }
            }

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
        }

        public class AttendanceReportItem
        {
            public string SubjectName { get; set; }
            public Dictionary<DateTime, string> DateData { get; set; }
            public int UPCount { get; set; }
            public int NCount { get; set; }

            public AttendanceReportItem()
            {
                DateData = new Dictionary<DateTime, string>();
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

        private void ToCreatePDF_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate = StartOfPeriod.SelectedDate ?? DateTime.MinValue;
            DateTime endDate = EndOfPeriod.SelectedDate ?? DateTime.MaxValue;

            int studentId = GetStudentId();

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

            // Создание списка уникальных дат и сортировка их по возрастанию
            var uniqueDates = reportData.Select(item => item.Date.Date).Distinct().OrderBy(date => date).ToList();

            // Создание списка объектов AttendanceReportItem
            List<AttendanceReportItem> reportItems = new List<AttendanceReportItem>();

            foreach (var item in reportData)
            {
                string passType = item.PassType ? "УП" : "Н";

                // Поиск соответствующего объекта AttendanceReportItem для предмета
                AttendanceReportItem reportItem = reportItems.FirstOrDefault(r => r.SubjectName == item.SubjectName);

                if (reportItem == null)
                {
                    reportItem = new AttendanceReportItem
                    {
                        SubjectName = item.SubjectName
                    };
                    reportItems.Add(reportItem);
                }

                // Заполнение данных о пропусках для каждой даты
                if (!reportItem.DateData.ContainsKey(item.Date.Date))
                {
                    reportItem.DateData.Add(item.Date.Date, passType);

                    if (passType == "УП")
                    {
                        reportItem.UPCount += 2;
                    }
                    else if (passType == "Н")
                    {
                        reportItem.NCount += 2;
                    }
                }
            }

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

                // Создание шрифта с русским базовым шрифтом
                Font russianFont = new Font(russianBaseFont, 9, Font.NORMAL);
                Font russianFont2 = new Font(russianBaseFont, 8, Font.NORMAL);
                Font russianFont1 = new Font(russianBaseFont, 20f, Font.BOLD);

                // Добавление заголовка
                Paragraph title = new Paragraph("Отчет об посещаемости студента", russianFont1)
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
                PdfPCell subjectCell = new PdfPCell(new Phrase("Предмет", russianFont))
                {
                    BackgroundColor = new BaseColor(230, 230, 230)
                };
                table.AddCell(subjectCell);

                foreach (var date in uniqueDates)
                {
                    PdfPCell dateCell = new PdfPCell(new Phrase(date.ToString("dd.MM.yy"), russianFont));
                    table.AddCell(dateCell);
                }


                // Добавление столбца "Отсутствие по уважительной причине"
                PdfPCell upCountCell = new PdfPCell(new Phrase("Отсутствие\nпо уважительной\nпричине", russianFont2))
                {
                    BackgroundColor = new BaseColor(230, 230, 230)
                };
                table.AddCell(upCountCell);

                // Добавление столбца "Отсутствие по неуважительной причине"
                PdfPCell nCountCell = new PdfPCell(new Phrase("Отсутствие\nпо неуважительной\nпричине", russianFont2))
                {
                    BackgroundColor = new BaseColor(230, 230, 230)
                };
                table.AddCell(nCountCell);


                PdfPCell upCountCell1; // Объявление переменной upCountCell перед циклом
                PdfPCell nCountCell1; // Объявление переменной nCountCell перед циклом

                // Добавление строк и данных таблицы
                foreach (AttendanceReportItem reportItem in reportItems)
                {
                    PdfPCell subjectName = new PdfPCell(new Phrase(reportItem.SubjectName, russianFont));
                    table.AddCell(subjectName);

                    foreach (var date in uniqueDates)
                    {
                        string passType = reportItem.DateData.ContainsKey(date) ? reportItem.DateData[date] : "";
                        PdfPCell passTypePDF = new PdfPCell(new Phrase(passType, russianFont))
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER // Центрирование содержимого
                        };
                        table.AddCell(passTypePDF); // Добавление ячейки с типом пропуска для каждой даты
                    }

                    // Присваивание значений переменным upCountCell и nCountCell
                    upCountCell1 = new PdfPCell(new Phrase(reportItem.UPCount.ToString(), russianFont));
                    nCountCell1 = new PdfPCell(new Phrase(reportItem.NCount.ToString(), russianFont));

                    // Добавление ячеек с количеством уважительных и неуважительных пропусков
                    table.AddCell(upCountCell1);
                    table.AddCell(nCountCell1);
                }

                document.Add(table);

                // Закрытие документа
                document.Close();
            }
        }


    }
}
