using ClosedXML.Excel;
using System.Collections.Generic;
using System.Drawing;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.ViewModels.Reports.Cars
{
	public class CarTechInspectReport
	{
		public static string ReportTitle => "Контроль прохождения ТО";

		private const string _dateFormatString = "dd.MM.yyyy";
		private readonly XLColor _headersBgColor = XLColor.FromColor(Color.FromArgb(170, 200, 140));

		private CarTechInspectReport() { }

		public IEnumerable<CarTechInspectNode> Items { get; private set; }

		private void ExportReport(string path)
		{
			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("ТО ТС");

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
			worksheet.Column(2).Width = 15;
			worksheet.Column(3).Width = 15;
			worksheet.Column(4).Width = 15;
			worksheet.Column(5).Width = 30;
			worksheet.Column(6).Width = 40;
			worksheet.Column(7).Width = 30;
			worksheet.Column(8).Width = 15;
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
			worksheet.Cell(rowNumber, colNumber++).Value = "№ п/п";
			worksheet.Cell(rowNumber, colNumber++).Value = "Тип авто";
			worksheet.Cell(rowNumber, colNumber++).Value = "Гос. номер ТС";
			worksheet.Cell(rowNumber, colNumber++).Value = "География";
			worksheet.Cell(rowNumber, colNumber++).Value = "Последнее показание одометра";
			worksheet.Cell(rowNumber, colNumber++).Value = "Дата и время последнего показания";
			worksheet.Cell(rowNumber, colNumber++).Value = "Плановое ТО на км";
			worksheet.Cell(rowNumber, colNumber).Value = "Остаток";

			var tableHeadersRange = worksheet.Range(rowNumber, 1, rowNumber, colNumber);
			FormatTableHeaderCells(tableHeadersRange);
		}

		private void FormatTableHeaderCells(IXLRange cellsRange)
		{
			cellsRange.AddConditionalFormat().WhenNotBlank().Fill.BackgroundColor = _headersBgColor;
			cellsRange.AddConditionalFormat().WhenIsBlank().Fill.BackgroundColor = _headersBgColor;
			cellsRange.Cells().Style.Font.Bold = true;
			cellsRange.Cells().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			cellsRange.Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
		}

		private void AddTableDataRows(IXLWorksheet worksheet, int rowNumber)
		{
			var startRowNumber = rowNumber;
			foreach(var inspect in Items)
			{
				var colNumber = 1;
				worksheet.Cell(rowNumber, colNumber++).Value = rowNumber - startRowNumber + 1;
				worksheet.Cell(rowNumber, colNumber++).Value = inspect.CarTypeOfUse.GetEnumDisplayName();
				worksheet.Cell(rowNumber, colNumber++).Value = inspect.CarRegNumber;
				worksheet.Cell(rowNumber, colNumber++).Value = inspect.DriverGeography;
				worksheet.Cell(rowNumber, colNumber++).Value =
					inspect.LastOdometerReading == null
					? 0
					: inspect.LastOdometerReading.Odometer;
				worksheet.Cell(rowNumber, colNumber++).Value =
					inspect.LastOdometerReading == null
					? ""
					: inspect.LastOdometerReading.StartDate.ToString(_dateFormatString);
				worksheet.Cell(rowNumber, colNumber++).Value = inspect.UpcomingTechInspectKm;
				worksheet.Cell(rowNumber, colNumber++).Value = inspect.LeftUntilTechInspectKm;
				rowNumber++;
			}

			var tableDataRange = worksheet.Range(startRowNumber, 1, rowNumber - 1, 8);
			FormatTableDataCells(tableDataRange);
		}

		private void FormatTableDataCells(IXLRange cellsRange)
		{
			cellsRange.Cells().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			cellsRange.Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
		}

		public static void ExportToExcel(string path, IEnumerable<CarTechInspectNode> items)
		{
			var carTechInspectReport = new CarTechInspectReport
			{
				Items = items
			};

			carTechInspectReport.ExportReport(path);
		}
	}
}
