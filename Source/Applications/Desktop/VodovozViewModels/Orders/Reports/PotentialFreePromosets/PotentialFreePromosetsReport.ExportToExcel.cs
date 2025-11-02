using ClosedXML.Excel;

namespace Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets
{
	public partial class PotentialFreePromosetsReport
	{
		public void ExportToExcel(string path)
		{
			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Отчет");

				SetColumnsWidth(worksheet);

				var excelRowCounter = 1;

				AddTableTitleRow(worksheet, excelRowCounter);
				excelRowCounter += 2;

				SetTableBordersAndAlignment(worksheet, excelRowCounter);

				AddTableHeadersRow(worksheet, excelRowCounter);
				excelRowCounter++;

				AddTableDataRows(worksheet, excelRowCounter);

				var rowsCount = excelRowCounter + Rows.Count;

				SetPrintParameters(worksheet);

				workbook.SaveAs(path);
			}
		}

		private void SetColumnsWidth(IXLWorksheet worksheet)
		{
			var colNumber = 1;

			worksheet.Column(colNumber++).Width = 5;
			worksheet.Column(colNumber++).Width = 40;
			worksheet.Column(colNumber++).Width = 20;
			worksheet.Column(colNumber++).Width = 20;
			worksheet.Column(colNumber++).Width = 25;
			worksheet.Column(colNumber++).Width = 10;
			worksheet.Column(colNumber++).Width = 15;
			worksheet.Column(colNumber++).Width = 15;
			worksheet.Column(colNumber++).Width = 25;
			worksheet.Column(colNumber++).Width = 15;
		}

		private void SetPrintParameters(IXLWorksheet worksheet)
		{
			worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
			worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
			worksheet.PageSetup.PagesWide = 1;
		}

		private void AddTableTitleRow(IXLWorksheet worksheet, int rowNumber)
		{
			worksheet.Cell(rowNumber, 1).Value = Title;

			var tableTitleRange = worksheet.Range(rowNumber, 1, rowNumber, 10);
			tableTitleRange.Merge();
			FormatTitleCells(tableTitleRange);
		}

		private void FormatTitleCells(IXLRange cellsRange)
		{
			cellsRange.Cells().Style.Font.FontSize = 14;
		}

		private void AddTableHeadersRow(IXLWorksheet worksheet, int rowNumber)
		{
			var colNumber = 1;
			worksheet.Cell(rowNumber, colNumber++).Value = "№";
			worksheet.Cell(rowNumber, colNumber++).Value = "Адрес";
			worksheet.Cell(rowNumber, colNumber++).Value = "Тип объекта";
			worksheet.Cell(rowNumber, colNumber++).Value = "Телефон";
			worksheet.Cell(rowNumber, colNumber++).Value = "Клиент";
			worksheet.Cell(rowNumber, colNumber++).Value = "Заказ";
			worksheet.Cell(rowNumber, colNumber++).Value = "Дата создания";
			worksheet.Cell(rowNumber, colNumber++).Value = "Дата заказа";
			worksheet.Cell(rowNumber, colNumber++).Value = "Промонабор";
			worksheet.Cell(rowNumber, colNumber++).Value = "Автор";
		}

		private void AddTableDataRows(IXLWorksheet worksheet, int rowNumber)
		{
			var startRowNumber = rowNumber;
			foreach(var row in Rows)
			{
				var colNumber = 1;
				worksheet.Cell(rowNumber, colNumber++).Value = row.SequenceNumber;
				worksheet.Cell(rowNumber, colNumber++).Value = row.Address;
				worksheet.Cell(rowNumber, colNumber++).Value = row.AddressCategory;
				worksheet.Cell(rowNumber, colNumber++).Value = row.Phone;
				worksheet.Cell(rowNumber, colNumber++).Value = row.Client;
				worksheet.Cell(rowNumber, colNumber++).Value = row.Order;
				worksheet.Cell(rowNumber, colNumber++).Value = row.OrderCreationDate;
				worksheet.Cell(rowNumber, colNumber++).Value = row.OrderDeliveryDate;
				worksheet.Cell(rowNumber, colNumber++).Value = row.Promoset;
				worksheet.Cell(rowNumber, colNumber).Value = row.Author;

				if(row.IsRootRow)
				{
					var cellsRange = worksheet.Range(rowNumber, 1, rowNumber, 10);
					SetCellsFontBold(cellsRange);
				}

				rowNumber++;
			}
		}

		private void SetTableBordersAndAlignment(IXLWorksheet worksheet, int tableStartRow)
		{
			var tableEndRow = tableStartRow + Rows.Count;

			var tableHeadersRange = worksheet.Range(tableStartRow, 1, tableStartRow, 10);
			FormatTableHeaderCells(tableHeadersRange);

			var tableDataRange = worksheet.Range(tableStartRow + 1, 1, tableEndRow, 10);
			FormatTableDataCells(tableDataRange);
		}

		private void FormatTableHeaderCells(IXLRange cellsRange)
		{
			SetCellsFontBold(cellsRange);

			cellsRange.Cells().Style.Alignment.WrapText = true;
			cellsRange.Cells().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			cellsRange.Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
		}

		private void FormatTableDataCells(IXLRange cellsRange)
		{
			cellsRange.Cells().Style.Alignment.WrapText = true;
			cellsRange.Cells().Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			cellsRange.Cells().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
		}

		private void SetCellsFontBold(IXLRange cellsRange)
		{
			cellsRange.Cells().Style.Font.Bold = true;
		}
	}
}
