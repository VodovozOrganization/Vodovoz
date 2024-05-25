using ClosedXML.Excel;
using System.Collections.Generic;
using System.Drawing;
using Vodovoz.Extensions;
using static Vodovoz.EntityRepositories.Logistic.CarRepository;

namespace Vodovoz.ViewModels.ViewModels.Reports.Cars
{
	public class CarInsurancesReport
	{
		public static string ReportTitle => "Контроль страховок";

		private const string _dateFormatString = "dd.MM.yyyy";
		private readonly XLColor _headersBgColor = XLColor.FromColor(Color.FromArgb(170, 200, 140));

		private CarInsurancesReport() { }

		public IEnumerable<CarInsuranceNode> Items { get; private set; }

		private void ExportReport(string path)
		{
			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Страховки ТС");

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
			worksheet.Column(5).Width = 15;
			worksheet.Column(6).Width = 15;
			worksheet.Column(7).Width = 30;
			worksheet.Column(8).Width = 20;
			worksheet.Column(9).Width = 10;
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
			worksheet.Cell(rowNumber, colNumber++).Value = "Гос. номер ТС";
			worksheet.Cell(rowNumber, colNumber++).Value = "География";
			worksheet.Cell(rowNumber, colNumber++).Value = "Тип страховки";
			worksheet.Cell(rowNumber, colNumber++).Value = "Начало";
			worksheet.Cell(rowNumber, colNumber++).Value = "Окончание";
			worksheet.Cell(rowNumber, colNumber++).Value = "Страховщик";
			worksheet.Cell(rowNumber, colNumber++).Value = "Номер";
			worksheet.Cell(rowNumber, colNumber).Value = "Осталось";

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
			var colNumber = 1;
			foreach(var insurance in Items)
			{
				worksheet.Cell(rowNumber, colNumber++).Value = rowNumber - startRowNumber + 1;
				worksheet.Cell(rowNumber, colNumber++).Value = insurance.CarRegNumber;
				worksheet.Cell(rowNumber, colNumber++).Value = insurance.DriverGeography;
				worksheet.Cell(rowNumber, colNumber++).Value = insurance.CarInsuranceType.GetEnumDisplayName();
				worksheet.Cell(rowNumber, colNumber++).Value = insurance.StartDate.ToString(_dateFormatString);
				worksheet.Cell(rowNumber, colNumber++).Value = insurance.EndDate.ToString(_dateFormatString);
				worksheet.Cell(rowNumber, colNumber++).Value = insurance.Insurer;
				worksheet.Cell(rowNumber, colNumber++).Value = insurance.InsuranceNumber;
				worksheet.Cell(rowNumber, colNumber).Value = insurance.DaysToExpire;
				rowNumber++;
			}

			var tableDataRange = worksheet.Range(startRowNumber, 1, rowNumber - 1, colNumber);
			FormatTableDataCells(tableDataRange);
		}

		private void FormatTableDataCells(IXLRange cellsRange)
		{
			cellsRange.Cells().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			cellsRange.Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
		}

		public static void ExportToExcel(string path, IEnumerable<CarInsuranceNode> items)
		{
			var carInsurancesReport = new CarInsurancesReport
			{
				Items = items
			};

			carInsurancesReport.ExportReport(path);
		}
	}
}
