using ClosedXML.Excel;
using System.Collections.Generic;
using System.Drawing;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.ViewModels.Reports.Cars
{
	public class CarInsurancesReport
	{
		public static string ReportTitle => "Контроль страховок";

		private const int _columnsCount = 10;
		private const string _dateFormatString = "dd.MM.yyyy";
		private readonly XLColor _headersBgColor = XLColor.FromColor(Color.FromArgb(170, 200, 140));
		private readonly XLColor _noCarInsuranceBgColor = XLColor.FromColor(Color.FromArgb(200, 50, 50));

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
			worksheet.Column(7).Width = 15;
			worksheet.Column(8).Width = 30;
			worksheet.Column(9).Width = 20;
			worksheet.Column(10).Width = 10;
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
				var isCarHasInsurance = !(item.LastInsurance is null);
				var colNumber = 1;
				worksheet.Cell(rowNumber, colNumber++).Value = rowNumber - startRowNumber + 1;
				worksheet.Cell(rowNumber, colNumber++).Value = item.CarTypeOfUse.GetEnumDisplayName();
				worksheet.Cell(rowNumber, colNumber++).Value = item.CarRegNumber;
				worksheet.Cell(rowNumber, colNumber++).Value = item.DriverGeography;
				worksheet.Cell(rowNumber, colNumber++).Value = item.InsuranceType.GetEnumDisplayName();

				if(isCarHasInsurance)
				{
					worksheet.Cell(rowNumber, colNumber++).Value = item.LastInsurance.StartDate.ToString(_dateFormatString);
					worksheet.Cell(rowNumber, colNumber++).Value = item.LastInsurance.EndDate.ToString(_dateFormatString);
					worksheet.Cell(rowNumber, colNumber++).Value = item.LastInsurance.Insurer.Name;
					worksheet.Cell(rowNumber, colNumber++).Value = item.LastInsurance.InsuranceNumber;
				}

				worksheet.Cell(rowNumber, _columnsCount).Value = item.DaysToExpire;

				if(!isCarHasInsurance)
				{
					var insuranceDataRange = worksheet.Range(rowNumber, 1, rowNumber, _columnsCount);
					FillCellBackground(insuranceDataRange, _noCarInsuranceBgColor);
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
