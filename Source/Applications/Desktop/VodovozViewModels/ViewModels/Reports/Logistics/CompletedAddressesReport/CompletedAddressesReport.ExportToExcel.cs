using ClosedXML.Excel;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.CompletedAddressesReport
{
	public partial class CompletedAddressesReport
	{
		private const string _headerColor = "#244462";
		private const string _totalColor = "#EAF0F6";
		private const int _headerRow = 4;

		/// <summary>
		/// Выгрузка отчета в файл Excel
		/// </summary>
		public void ExportToExcel(string path)
		{
			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Отчет");

				var districtsCount = DistrictTitles.Count;
				var firstDistrictColumn = 2;
				var lastDistrictColumn = firstDistrictColumn + districtsCount - 1;
				var totalColumn = lastDistrictColumn + 1;
				var lastRow = _headerRow + Rows.Count;

				worksheet.Style.Font.FontName = "Arial";
				worksheet.Style.Font.FontSize = 10;

				worksheet.Column(1).Width = 48;
				for(var column = firstDistrictColumn; column <= lastDistrictColumn; column++)
				{
					worksheet.Column(column).Width = 20;
				}
				worksheet.Column(totalColumn).Width = 28;

				worksheet.Cell(1, 2).Value = Title;
				var titleRange = worksheet.Range(1, 2, 1, totalColumn - 1 > 2 ? totalColumn - 1 : 2);
				titleRange.Merge();
				titleRange.Style.Font.Bold = true;
				titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

				worksheet.Cell(_headerRow, 1).Value = "ФИО";
				for(var i = 0; i < districtsCount; i++)
				{
					worksheet.Cell(_headerRow, firstDistrictColumn + i).Value = DistrictTitles[i];
				}
				worksheet.Cell(_headerRow, totalColumn).Value = "Суммарно адресов по всем районам";

				var headerRange = worksheet.Range(_headerRow, 1, _headerRow, totalColumn);
				headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml(_headerColor);
				headerRange.Style.Font.FontColor = XLColor.White;
				headerRange.Style.Font.Bold = true;
				headerRange.Style.Alignment.WrapText = true;

				var rowNumber = _headerRow + 1;
				foreach(var row in Rows)
				{
					worksheet.Cell(rowNumber, 1).Value = row.DriverFullName;
					for(var i = 0; i < districtsCount; i++)
					{
						worksheet.Cell(rowNumber, firstDistrictColumn + i).Value = row.DistrictCounts[i];
					}

					var firstCell = worksheet.Cell(rowNumber, firstDistrictColumn).Address.ToStringRelative();
					var lastCell = worksheet.Cell(rowNumber, lastDistrictColumn).Address.ToStringRelative();
					var totalCell = worksheet.Cell(rowNumber, totalColumn);
					totalCell.FormulaA1 = districtsCount > 0 ? $"SUM({firstCell}:{lastCell})" : "0";
					totalCell.Style.Font.Bold = true;
					totalCell.Style.Fill.BackgroundColor = XLColor.FromHtml(_totalColor);

					rowNumber++;
				}

				var tableRange = worksheet.Range(_headerRow, 1, lastRow, totalColumn);
				tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
				tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
				tableRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
				tableRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

				if(lastRow > _headerRow)
				{
					worksheet.Range(_headerRow + 1, firstDistrictColumn, lastRow, totalColumn).Style.NumberFormat.Format = "#,##0";
				}

				worksheet.SheetView.FreezeRows(_headerRow);

				workbook.SaveAs(path);
			}
		}
	}
}
