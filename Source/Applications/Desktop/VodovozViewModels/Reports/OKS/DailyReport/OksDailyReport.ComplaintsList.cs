using ClosedXML.Excel;
using Gamma.Utilities;
using MoreLinq;
using System.Drawing;
using System.Linq;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.ViewModels.Reports.OKS.DailyReport
{
	public partial class OksDailyReport
	{
		private const int _complaintsListTableStartColumnNumber = 1;
		private int _complaintsListNextEmptyRowNumber = 0;
		private readonly XLColor _complaintsResultMarkupBgColor = XLColor.FromColor(Color.FromArgb(146, 208, 80));

		private void FillComplaintsListWorksheet(ref IXLWorksheet worksheet)
		{
			_complaintsListNextEmptyRowNumber = 2;

			SetComplaintsListColumnsWidth(ref worksheet);

			AddComplaintsListTitleTable(ref worksheet);
			AddComplaintsListTableHeaders(ref worksheet);
			AddComplaintsListTableData(ref worksheet);
		}

		private void SetComplaintsListColumnsWidth(ref IXLWorksheet worksheet)
		{
			worksheet.Column(1).Width = 8;
			worksheet.Column(2).Width = 8;
			worksheet.Column(3).Width = 12;
			worksheet.Column(4).Width = 10;
			worksheet.Column(5).Width = 12;
			worksheet.Column(6).Width = 24;
			worksheet.Column(7).Width = 16;
			worksheet.Column(8).Width = 16;
			worksheet.Column(9).Width = 36;
			worksheet.Column(10).Width = 72;
			worksheet.Column(11).Width = 36;
		}

		private void AddComplaintsListTitleTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = _complaintsListNextEmptyRowNumber;

			worksheet.Cell(rowNumber, _complaintsListTableStartColumnNumber).Value = $"Рекламации, поступившие {_date.ToString(_dateFormatString)}";

			var cellsRange = worksheet.Range(rowNumber, _complaintsListTableStartColumnNumber, rowNumber, 11);
			cellsRange.Merge();
			FormatComplaintsWorksheetsTitleCells(cellsRange);

			_complaintsListNextEmptyRowNumber = 4;
		}

		private void AddComplaintsListTableHeaders(ref IXLWorksheet worksheet)
		{
			var rowNumber = _complaintsListNextEmptyRowNumber;

			var columnNumber = _complaintsListTableStartColumnNumber;
			worksheet.Cell(rowNumber, columnNumber++).Value = "№";
			worksheet.Cell(rowNumber, columnNumber++).Value = "Рекламация №";
			worksheet.Cell(rowNumber, columnNumber++).Value = "Дата";
			worksheet.Cell(rowNumber, columnNumber++).Value = "Время создания";
			worksheet.Cell(rowNumber, columnNumber++).Value = "Статус";
			worksheet.Cell(rowNumber, columnNumber++).Value = "Клиент";
			worksheet.Cell(rowNumber, columnNumber++).Value = "Объект";
			worksheet.Cell(rowNumber, columnNumber++).Value = "Вид";
			worksheet.Cell(rowNumber, columnNumber++).Value = "Проблема";
			worksheet.Cell(rowNumber, columnNumber++).Value = "Что сделали";
			worksheet.Cell(rowNumber, columnNumber).Value = "Результат";

			var tableHeaderCellsRange = worksheet.Range(rowNumber, _complaintsListTableStartColumnNumber, rowNumber, columnNumber);
			FormatComplaintsBoldFontMediumBordersWithBackgroundCells(tableHeaderCellsRange);

			_complaintsListNextEmptyRowNumber = 5;
		}

		private void AddComplaintsListTableData(ref IXLWorksheet worksheet)
		{
			var startRowNumber = _complaintsListNextEmptyRowNumber;
			var rowNumber = startRowNumber;
			var columnNumber = _complaintsListTableStartColumnNumber;
			var complaintsCounter = 0;

			foreach(var complaint in _complaintsDataOnDate)
			{
				var resultComments =
					string.Join(" || ",
					complaint.ComplaintResults.DistinctBy(c => c.ComplaintResultCommentId).Select(r => r.ComplaintResultComment));

				columnNumber = 1;
				worksheet.Cell(rowNumber, columnNumber++).Value = ++complaintsCounter;
				worksheet.Cell(rowNumber, columnNumber++).Value = complaint.Id;
				worksheet.Cell(rowNumber, columnNumber++).Value = complaint.CreationDate.ToString("dd.MM.yyyy");
				worksheet.Cell(rowNumber, columnNumber++).Value = complaint.CreationDate.ToString("HH:mm");
				worksheet.Cell(rowNumber, columnNumber++).Value = complaint.Status.GetEnumTitle();
				worksheet.Cell(rowNumber, columnNumber++).Value = complaint.ClientName;
				worksheet.Cell(rowNumber, columnNumber++).Value = complaint.ComplaintObject?.Name;
				worksheet.Cell(rowNumber, columnNumber++).Value = complaint.ComplaintKind?.Name;
				worksheet.Cell(rowNumber, columnNumber++).Value = complaint.ComplaintText;
				worksheet.Cell(rowNumber, columnNumber++).Value = resultComments;
				worksheet.Cell(rowNumber, columnNumber).Value = complaint.WorkWithClientResult?.GetEnumTitle();

				if(complaint.WorkWithClientResult == ComplaintWorkWithClientResult.Solved)
				{
					FillCellBackground(
						worksheet.Range(rowNumber, columnNumber, rowNumber, columnNumber),
						_complaintsResultMarkupBgColor);
				}

				rowNumber++;
			}

			var tableDataCellsRange = worksheet.Range(startRowNumber, _complaintsListTableStartColumnNumber, rowNumber - 1, columnNumber);
			FormatComplaintsDataCommonCells(tableDataCellsRange, XLAlignmentHorizontalValues.Left);
		}
	}
}
