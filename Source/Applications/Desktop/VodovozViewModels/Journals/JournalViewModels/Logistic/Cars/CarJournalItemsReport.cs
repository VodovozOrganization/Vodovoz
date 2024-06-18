using ClosedXML.Excel;
using System.Collections.Generic;
using System.Drawing;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic.Cars
{
	public class CarJournalItemsReport
	{
		public static string ReportTitle => "Список автомобилей";

		private const int _columnsCount = 7;
		private readonly XLColor _headersBgColor = XLColor.FromColor(Color.FromArgb(170, 200, 140));
		private readonly XLColor _notificationBgColor = XLColor.FromColor(Color.FromArgb(250, 192, 192));

		private CarJournalItemsReport() { }

		public IEnumerable<CarJournalNode> Items { get; private set; }

		private void ExportReport(string path)
		{
			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Список автомобилей");

				SetColumnsWidth(worksheet);

				var excelRowCounter = 1;

				AddTableTitleRow(worksheet, excelRowCounter);
				excelRowCounter++;

				AddTableHeadersRow(worksheet, excelRowCounter);
				excelRowCounter++;

				AddTableDataRows(worksheet, excelRowCounter);

				workbook.SaveAs(path);
			}
		}

		private void SetColumnsWidth(IXLWorksheet worksheet)
		{
			worksheet.Column(1).Width = 10;
			worksheet.Column(2).Width = 40;
			worksheet.Column(3).Width = 30;
			worksheet.Column(4).Width = 30;
			worksheet.Column(5).Width = 30;
			worksheet.Column(6).Width = 40;
			worksheet.Column(7).Width = 70;
		}

		private void AddTableTitleRow(IXLWorksheet worksheet, int rowNumber)
		{
			worksheet.Cell(rowNumber, 1).Value = ReportTitle;

			var tableTitleRange = worksheet.Range(rowNumber, 1, rowNumber, 3);
			tableTitleRange.Merge();
			FormatTitleCells(tableTitleRange);
		}

		private void FormatTitleCells(IXLRange cellsRange)
		{
			cellsRange.Cells().Style.Font.Bold = true;
		}

		private void AddTableHeadersRow(IXLWorksheet worksheet, int rowNumber)
		{
			var colNumber = 1;
			worksheet.Cell(rowNumber, colNumber++).Value = "Код";
			worksheet.Cell(rowNumber, colNumber++).Value = "Собственник";
			worksheet.Cell(rowNumber, colNumber++).Value = "Производитель";
			worksheet.Cell(rowNumber, colNumber++).Value = "Модель";
			worksheet.Cell(rowNumber, colNumber++).Value = "Гос. номер";
			worksheet.Cell(rowNumber, colNumber++).Value = "Водитель";
			worksheet.Cell(rowNumber, colNumber++).Value = "Страховщик";

			var tableHeadersRange = worksheet.Range(rowNumber, 1, rowNumber, _columnsCount);
			FormatTableHeaderCells(tableHeadersRange);
		}

		private void FormatTableHeaderCells(IXLRange cellsRange)
		{
			FillCellBackground(cellsRange, _headersBgColor);

			cellsRange.Cells().Style.Font.Bold = true;
			cellsRange.Cells().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			cellsRange.Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
		}

		private void AddTableDataRows(IXLWorksheet worksheet, int rowNumber)
		{
			var startRowNumber = rowNumber;
			foreach(var item in Items)
			{
				var colNumber = 1;
				worksheet.Cell(rowNumber, colNumber++).Value = item.Id;
				worksheet.Cell(rowNumber, colNumber++).Value = item.CarOwner;
				worksheet.Cell(rowNumber, colNumber++).Value = item.ManufacturerName;
				worksheet.Cell(rowNumber, colNumber++).Value = item.ModelName;
				worksheet.Cell(rowNumber, colNumber++).Value = item.RegistrationNumber;
				worksheet.Cell(rowNumber, colNumber++).Value = item.DriverName;
				worksheet.Cell(rowNumber, colNumber++).Value = item.InsurersNames;

				if(item.IsShowBackgroundColorNotification)
				{
					var notifyCellsRange = worksheet.Range(rowNumber, 1, rowNumber, _columnsCount);
					FillCellBackground(notifyCellsRange, _notificationBgColor);
				}

				rowNumber++;
			}

			var tableDataRange = worksheet.Range(startRowNumber, 1, rowNumber - 1, _columnsCount);
			FormatTableDataCells(tableDataRange);
		}

		private void FormatTableDataCells(IXLRange cellsRange)
		{
			cellsRange.Cells().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			cellsRange.Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
		}

		private void FillCellBackground(IXLRange cellsRange, XLColor color)
		{
			cellsRange.AddConditionalFormat().WhenNotBlank().Fill.BackgroundColor = color;
			cellsRange.AddConditionalFormat().WhenIsBlank().Fill.BackgroundColor = color;
		}

		public static void ExportToExcel(string path, IEnumerable<CarJournalNode> items)
		{
			var report = new CarJournalItemsReport
			{
				Items = items
			};

			report.ExportReport(path);
		}
	}
}
