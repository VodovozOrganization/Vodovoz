using ClosedXML.Excel;
using QS.Project.Services.FileDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.ViewModels.ViewModels.Reports.DebtorsJournalReport
{
	public class DebtorsJournalReport
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly Report _report;

		public DebtorsJournalReport(IList<DebtorJournalNode> rows, IFileDialogService fileDialogService)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_report = new Report(rows);
		}

		public void Export()
		{
			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileName = $"{"Журнал задолженностей"} {DateTime.Now:yyyy-MM-dd-HH-mm}.xlsx";

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(result.Successful)
			{
				SaveReport(result.Path);
			}
		}

		private void SaveReport(string path)
		{
			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Журнал задолженностей");

				worksheet.Column(1).Width = 5;
				worksheet.Column(2).Width = 15;
				worksheet.Column(3).Width = 40;
				worksheet.Column(4).Width = 75;
				worksheet.Column(5).Width = 15;
				worksheet.Column(6).Width = 30;
				worksheet.Column(7).Width = 15;
				worksheet.Column(8).Width = 10;
				worksheet.Column(9).Width = 12;
	
				worksheet.Cell(1, 1).Value = "№";
				worksheet.Cell(1, 2).Value = "Код контрагента";
				worksheet.Cell(1, 3).Value = "Клиент";
				worksheet.Cell(1, 4).Value = "Адрес";
				worksheet.Cell(1, 5).Value = "Номер телефона";
				worksheet.Cell(1, 6).Value = "Email";
				worksheet.Cell(1, 7).Value = "Дата последнего заказа";
				worksheet.Cell(1, 8).Value = "Долг по таре\n(по адресу) бутылей";
				worksheet.Cell(1, 9).Value = "Кол-во отгруженных\nв последнюю реализацию\nбутылей";

				var rows = _report.Rows;

				for(int i = 0; i < rows.Count; i++)
				{
					worksheet.Cell(i + 2, 1).Value = i + 1;
					worksheet.Cell(i + 2, 2).Value = rows[i].ClientId;
					worksheet.Cell(i + 2, 3).Value = rows[i].ClientName;
					worksheet.Cell(i + 2, 4).Value = rows[i].Address;
					worksheet.Cell(i + 2, 5).Value = rows[i].Phones;
					worksheet.Cell(i + 2, 6).Value = rows[i].Emails;
					worksheet.Cell(i + 2, 7).Value = rows[i].LastOrderDate;
					worksheet.Cell(i + 2, 8).Value = rows[i].DebtByAddress;
					worksheet.Cell(i + 2, 9).Value = rows[i].LastOrderBottles;
				}

				worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

				worksheet.Cell(rows.Count + 2, 5).Value = "Итого:";
				worksheet.Cell(rows.Count + 2, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
				worksheet.Cell(rows.Count + 2, 6).Value = rows.Sum(x => x.DebtByAddress);
				worksheet.Cell(rows.Count + 2, 7).Value = rows.Sum(x => x.LastOrderBottles);

				for(int c = 1; c <= 7; c++)
				{
					for(int r = 1; r <= rows.Count + 1; r++)
					{
						worksheet.Cell(r , c).Style.Alignment.WrapText = true;
					}
				}

				workbook.SaveAs(path);
			}
		}

		private class Report
		{
			public Report(IList<DebtorJournalNode> rows)
			{
				Rows = rows;
			}

			public IList<DebtorJournalNode> Rows { get; }
		}
	}
}
