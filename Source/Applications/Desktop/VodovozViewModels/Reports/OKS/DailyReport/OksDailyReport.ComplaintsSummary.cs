using ClosedXML.Excel;
using DateTimeHelpers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories.Complaints;

namespace Vodovoz.ViewModels.Reports.OKS.DailyReport
{
	public partial class OksDailyReport
	{
		private readonly XLColor _complaintsSummaryMarkupBgColor = XLColor.FromColor(Color.FromArgb(226, 240, 217));

		public IList<OksDailyReportComplaintDataNode> ComplaintsDataForDate { get; private set; } =
			new List<OksDailyReportComplaintDataNode>();
		public IList<OksDailyReportComplaintDataNode> ComplaintsDataFromMonthBeginningToDate { get; private set; } =
			new List<OksDailyReportComplaintDataNode>();

		private void FillComplaintsSummaryWorksheet(ref IXLWorksheet worksheet)
		{
			SetComplaintsSummaryColumnsWidth(ref worksheet);
			AddComplaintsSummaryTitleTable(ref worksheet);
			AddComplaintsSummaryByComplaintSourceTable(ref worksheet);
			AddComplaintsSummaryByComplaintResultTable(ref worksheet);
			AddComplaintsSummaryByComplaintStatusOnDateTable(ref worksheet);
			AddComplaintsSummaryByComplaintStatusFromMonthStartTable(ref worksheet);
			AddComplaintsSummaryTypesAndObjectsTable(ref worksheet);
		}

		private void SetComplaintsSummaryColumnsWidth(ref IXLWorksheet worksheet)
		{
			worksheet.Column(1).Width = 8;
			worksheet.Column(2).Width = 24;
			worksheet.Column(3).Width = 12;
			worksheet.Column(4).Width = 8;
			worksheet.Column(5).Width = 24;
			worksheet.Column(6).Width = 12;
			worksheet.Column(7).Width = 8;
			worksheet.Column(8).Width = 12;
			worksheet.Column(9).Width = 12;
			worksheet.Column(10).Width = 12;
			worksheet.Column(11).Width = 8;
			worksheet.Column(12).Width = 12;
			worksheet.Column(13).Width = 12;
			worksheet.Column(14).Width = 12;
		}

		private void AddComplaintsSummaryTitleTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = 2;

			worksheet.Cell(rowNumber, 2).Value = $"Отчет по рекламациям ОКС {Date.ToString(_dateFormatString)}";

			var cellsRange = worksheet.Range(rowNumber, 2, rowNumber, 14);
			cellsRange.Merge();

			FormatComplaintsSummaryWorksheetTitleCells(cellsRange);
		}

		private void AddComplaintsSummaryByComplaintSourceTable(ref IXLWorksheet worksheet)
		{
			var startRowNumber = 4;
			var labelColumnNumber = 2;
			var dataColumnNumber = labelColumnNumber + 1;
			var rowNumber = startRowNumber;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = "Всего рекламаций за смену:";
			worksheet.Cell(rowNumber, dataColumnNumber).Value = ComplaintsDataForDate.Count;

			rowNumber++;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = " - Входящие звонки";
			worksheet.Cell(rowNumber, dataColumnNumber).Value = ComplaintsDataForDate.Where(c => c.ComplaintSource.Id == IncomingCallSourseId).Count();

			rowNumber++;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = " - Чат \"Обращения\"";
			worksheet.Cell(rowNumber, dataColumnNumber).Value = ComplaintsDataForDate.Where(c => c.ComplaintSource.Id != IncomingCallSourseId).Count();

			FormatComplaintsSummaryLabelCells(worksheet.Range(startRowNumber, labelColumnNumber, rowNumber, labelColumnNumber));
			FormatComplaintsSummaryDataCells(worksheet.Range(startRowNumber, dataColumnNumber, rowNumber, dataColumnNumber));
		}

		private void AddComplaintsSummaryByComplaintResultTable(ref IXLWorksheet worksheet)
		{
			var startRowNumber = 4;
			var labelColumnNumber = 5;
			var dataColumnNumber = labelColumnNumber + 1;
			var rowNumber = startRowNumber;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = "Проблема решена";
			worksheet.Cell(rowNumber, dataColumnNumber).Value =
				ComplaintsDataForDate
				.Where(c => c.WorkWithClientResult == VodovozBusiness.Domain.Complaints.ComplaintWorkWithClientResult.Solved)
				.Count();

			rowNumber++;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = "Проблема НЕ решена";
			worksheet.Cell(rowNumber, dataColumnNumber).Value =
				ComplaintsDataForDate
				.Where(c => c.WorkWithClientResult == VodovozBusiness.Domain.Complaints.ComplaintWorkWithClientResult.NotSolved)
				.Count();

			rowNumber++;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = "Результат не указан";
			worksheet.Cell(rowNumber, dataColumnNumber).Value =
				ComplaintsDataForDate
				.Where(c => c.WorkWithClientResult is null)
				.Count();

			FormatComplaintsSummaryLabelCells(worksheet.Range(startRowNumber, labelColumnNumber, rowNumber, labelColumnNumber));
			FormatComplaintsSummaryDataCells(worksheet.Range(startRowNumber, dataColumnNumber, rowNumber, dataColumnNumber));
		}

		private void AddComplaintsSummaryByComplaintStatusOnDateTable(ref IXLWorksheet worksheet)
		{
			var startRowNumber = 4;
			var firstColumnNumber = 8;
			var secondColumnNumber = firstColumnNumber + 1;
			var thirdColumnNumber = firstColumnNumber + 2;
			var rowNumber = startRowNumber;

			worksheet.Cell(rowNumber, firstColumnNumber).Value = "Статус рекламаций за смену";
			worksheet.Range(rowNumber, firstColumnNumber, rowNumber, thirdColumnNumber).Merge();

			rowNumber++;

			worksheet.Cell(rowNumber, firstColumnNumber).Value = "В работе";
			worksheet.Cell(rowNumber, secondColumnNumber).Value = "На проверке";
			worksheet.Cell(rowNumber, thirdColumnNumber).Value = "Закрыт";

			rowNumber++;

			worksheet.Cell(rowNumber, firstColumnNumber).Value =
				ComplaintsDataForDate
				.Where(c => c.OksDiskussionStatuse == ComplaintDiscussionStatuses.InProcess)
				.Count();

			worksheet.Cell(rowNumber, secondColumnNumber).Value =
				ComplaintsDataForDate
				.Where(c => c.OksDiskussionStatuse == ComplaintDiscussionStatuses.Checking)
				.Count();

			worksheet.Cell(rowNumber, thirdColumnNumber).Value =
				ComplaintsDataForDate
				.Where(c => c.OksDiskussionStatuse == ComplaintDiscussionStatuses.Closed)
				.Count();

			FormatComplaintsSummaryTableHeaerCells(worksheet.Range(startRowNumber, firstColumnNumber, startRowNumber + 1, thirdColumnNumber));
			FormatComplaintsSummaryDataCells(worksheet.Range(rowNumber, firstColumnNumber, rowNumber, thirdColumnNumber));
		}

		private void AddComplaintsSummaryByComplaintStatusFromMonthStartTable(ref IXLWorksheet worksheet)
		{
			var startRowNumber = 4;
			var firstColumnNumber = 12;
			var secondColumnNumber = firstColumnNumber + 1;
			var thirdColumnNumber = firstColumnNumber + 2;
			var rowNumber = startRowNumber;

			worksheet.Cell(rowNumber, firstColumnNumber).Value = $"Статус рекламаций с {Date.FirstDayOfMonth().ToString(_dateFormatString)} по {Date.ToString(_dateFormatString)}";
			worksheet.Range(rowNumber, firstColumnNumber, rowNumber, thirdColumnNumber).Merge();

			rowNumber++;

			worksheet.Cell(rowNumber, firstColumnNumber).Value = "В работе";
			worksheet.Cell(rowNumber, secondColumnNumber).Value = "На проверке";
			worksheet.Cell(rowNumber, thirdColumnNumber).Value = "Закрыт";

			rowNumber++;

			worksheet.Cell(rowNumber, firstColumnNumber).Value =
				ComplaintsDataFromMonthBeginningToDate
				.Where(c => c.OksDiskussionStatuse == ComplaintDiscussionStatuses.InProcess)
				.Count();

			worksheet.Cell(rowNumber, secondColumnNumber).Value =
				ComplaintsDataFromMonthBeginningToDate
				.Where(c => c.OksDiskussionStatuse == ComplaintDiscussionStatuses.Checking)
				.Count();

			worksheet.Cell(rowNumber, thirdColumnNumber).Value =
				ComplaintsDataFromMonthBeginningToDate
				.Where(c => c.OksDiskussionStatuse == ComplaintDiscussionStatuses.Closed)
				.Count();

			FormatComplaintsSummaryTableHeaerCells(worksheet.Range(startRowNumber, firstColumnNumber, startRowNumber + 1, thirdColumnNumber));
			FormatComplaintsSummaryDataCells(worksheet.Range(rowNumber, firstColumnNumber, rowNumber, thirdColumnNumber));
		}

		private void AddComplaintsSummaryTypesAndObjectsTable(ref IXLWorksheet worksheet)
		{
			var startRowNumber = 9;
			var objectNameColumnNumber = 2;
			var typeNameColumnNumber = 4;
			var dataColumnNumber = 10;
			var rowNumber = startRowNumber;

			worksheet.Cell(rowNumber, objectNameColumnNumber).Value = "Виды и объекты рекламаций";
			worksheet.Range(rowNumber, objectNameColumnNumber, rowNumber, dataColumnNumber).Merge();
			rowNumber++;

			var groupedByObjectItems = ComplaintsDataForDate
				.GroupBy(c => c.ComplaintObject)
				.ToDictionary(c => c.Key, c => c.ToList());

			foreach(var objectItems in groupedByObjectItems)
			{
				var groupedByTypeItems = objectItems.Value
					.GroupBy(c => c.ComplaintKind)
					.ToDictionary(c => c.Key, c => c.Count());

				var groupStartRowNumber = rowNumber;

				worksheet.Cell(rowNumber, objectNameColumnNumber).Value = $"{objectItems.Key?.Name ?? "Объект не указан"}";
				worksheet.Range(groupStartRowNumber, objectNameColumnNumber, groupStartRowNumber + groupedByTypeItems.Count, typeNameColumnNumber - 1).Merge();

				foreach(var typeItem in groupedByTypeItems)
				{
					worksheet.Cell(rowNumber, typeNameColumnNumber).Value = typeItem.Key.Name;
					worksheet.Cell(rowNumber, dataColumnNumber).Value = typeItem.Value;

					worksheet.Range(rowNumber, typeNameColumnNumber, rowNumber, dataColumnNumber - 1).Merge();

					rowNumber++;
				}
			}

			//FormatComplaintsSummaryObjectTypeDataCells(worksheet.Range(startRowNumber + 1, objectNameColumnNumber, rowNumber - 1, dataColumnNumber));
			FormatComplaintsSummaryTableHeaerCells(worksheet.Range(startRowNumber, objectNameColumnNumber, startRowNumber, typeNameColumnNumber - 1));
		}

		private void FormatComplaintsSummaryWorksheetTitleCells(IXLRange cellsRange)
		{
			FormatCells(
				cellsRange,
				fontSize: 22,
				isBoldFont: true,
				bgColor: _complaintsSummaryMarkupBgColor,
				horizontalAlignment: XLAlignmentHorizontalValues.Center,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatComplaintsSummaryTableHeaerCells(IXLRange cellsRange)
		{
			FormatCells(
				cellsRange,
				isBoldFont: true,
				bgColor: _complaintsSummaryMarkupBgColor,
				horizontalAlignment: XLAlignmentHorizontalValues.Center,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatComplaintsSummaryLabelCells(IXLRange cellsRange)
		{
			FormatCells(
				cellsRange,
				isBoldFont: true,
				bgColor: _complaintsSummaryMarkupBgColor,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatComplaintsSummaryDataCells(IXLRange cellsRange)
		{
			FormatCells(
				cellsRange,
				fontSize: 12,
				isBoldFont: true,
				horizontalAlignment: XLAlignmentHorizontalValues.Center,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatComplaintsSummaryObjectTypeDataCells(IXLRange cellsRange)
		{
			FormatCells(
				cellsRange,
				fontSize: 11,
				horizontalAlignment: XLAlignmentHorizontalValues.Center,
				cellBorderStyle: XLBorderStyleValues.Thin);
		}
	}
}
