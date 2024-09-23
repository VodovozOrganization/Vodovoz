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
		private readonly XLColor _complaintsTitlesMarkupBgColor = XLColor.FromColor(Color.FromArgb(226, 240, 217));

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

			FormatComplaintsWorksheetsTitleCells(cellsRange);
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

			FormatComplaintsBoldFontMediumBordersWithBackgroundCells(
				worksheet.Range(startRowNumber, labelColumnNumber, rowNumber, labelColumnNumber),
				XLAlignmentHorizontalValues.Left);

			FormatComplaintsBoldFontMediumBordersCells(
				worksheet.Range(startRowNumber, dataColumnNumber, rowNumber, dataColumnNumber));
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

			FormatComplaintsBoldFontMediumBordersWithBackgroundCells(
				worksheet.Range(startRowNumber, labelColumnNumber, rowNumber, labelColumnNumber),
				XLAlignmentHorizontalValues.Left);

			FormatComplaintsBoldFontMediumBordersCells(
				worksheet.Range(startRowNumber, dataColumnNumber, rowNumber, dataColumnNumber));
		}

		private void AddComplaintsSummaryByComplaintStatusOnDateTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = 4;
			var firstColumnNumber = 8;
			var secondColumnNumber = firstColumnNumber + 1;
			var thirdColumnNumber = firstColumnNumber + 2;

			worksheet.Cell(rowNumber, firstColumnNumber).Value = "Статус рекламаций за смену";
			var mainHeaderCellsRange = worksheet.Range(rowNumber, firstColumnNumber, rowNumber, thirdColumnNumber);
			mainHeaderCellsRange.Merge();
			FormatComplaintsBoldFontMediumBordersWithBackgroundCells(mainHeaderCellsRange);

			rowNumber++;

			worksheet.Cell(rowNumber, firstColumnNumber).Value = "В работе";
			worksheet.Cell(rowNumber, secondColumnNumber).Value = "На проверке";
			worksheet.Cell(rowNumber, thirdColumnNumber).Value = "Закрыт";

			var headerCellsRange = worksheet.Range(rowNumber, firstColumnNumber, rowNumber, thirdColumnNumber);
			FormatComplaintsBoldFontMediumBordersWithBackgroundCells(headerCellsRange);

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

			var dataCellsRange = worksheet.Range(rowNumber, firstColumnNumber, rowNumber, thirdColumnNumber);
			FormatComplaintsBoldFontMediumBordersCells(dataCellsRange);
		}

		private void AddComplaintsSummaryByComplaintStatusFromMonthStartTable(ref IXLWorksheet worksheet)
		{
			var startRowNumber = 4;
			var firstColumnNumber = 12;
			var secondColumnNumber = firstColumnNumber + 1;
			var thirdColumnNumber = firstColumnNumber + 2;
			var rowNumber = startRowNumber;

			worksheet.Cell(rowNumber, firstColumnNumber).Value =
				$"Статус рекламаций с {Date.FirstDayOfMonth().ToString(_dateFormatString)} по {Date.ToString(_dateFormatString)}";

			var mainHeaderCellsRange = worksheet.Range(rowNumber, firstColumnNumber, rowNumber, thirdColumnNumber);
			mainHeaderCellsRange.Merge();
			FormatComplaintsBoldFontMediumBordersWithBackgroundCells(mainHeaderCellsRange);

			rowNumber++;

			worksheet.Cell(rowNumber, firstColumnNumber).Value = "В работе";
			worksheet.Cell(rowNumber, secondColumnNumber).Value = "На проверке";
			worksheet.Cell(rowNumber, thirdColumnNumber).Value = "Закрыт";

			var headerCellsRange = worksheet.Range(rowNumber, firstColumnNumber, rowNumber, thirdColumnNumber);
			FormatComplaintsBoldFontMediumBordersWithBackgroundCells(headerCellsRange);

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

			var dataCellsRange = worksheet.Range(rowNumber, firstColumnNumber, rowNumber, thirdColumnNumber);
			FormatComplaintsBoldFontMediumBordersCells(dataCellsRange);
		}

		private void AddComplaintsSummaryTypesAndObjectsTable(ref IXLWorksheet worksheet)
		{
			var startRowNumber = 9;
			var objectNameColumnNumber = 2;
			var typeNameColumnNumber = 4;
			var dataColumnNumber = 10;
			var rowNumber = startRowNumber;

			worksheet.Cell(rowNumber, objectNameColumnNumber).Value = "Виды и объекты рекламаций";

			var headerCellsRange = worksheet.Range(rowNumber, objectNameColumnNumber, rowNumber, dataColumnNumber);
			headerCellsRange.Merge();
			FormatComplaintsBoldFontMediumBordersWithBackgroundCells(headerCellsRange);

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

				var groupByObjectCellsRange = worksheet
					.Range(groupStartRowNumber, objectNameColumnNumber, groupStartRowNumber + groupedByTypeItems.Count - 1, typeNameColumnNumber - 1);
				groupByObjectCellsRange.Merge();
				FormatComplaintsDataCommonCells(groupByObjectCellsRange);

				foreach(var typeItem in groupedByTypeItems)
				{
					worksheet.Cell(rowNumber, typeNameColumnNumber).Value = typeItem.Key.Name;
					worksheet.Cell(rowNumber, dataColumnNumber).Value = typeItem.Value;

					var typeNameCellsRange = worksheet.Range(rowNumber, typeNameColumnNumber, rowNumber, dataColumnNumber - 1);
					typeNameCellsRange.Merge();
					FormatComplaintsDataCommonCells(typeNameCellsRange, XLAlignmentHorizontalValues.Left);

					var dataCellsRange = worksheet.Range(rowNumber, dataColumnNumber, rowNumber, dataColumnNumber);
					FormatComplaintsDataCommonCells(dataCellsRange);

					rowNumber++;
				}
			}

			worksheet.Cell(rowNumber, dataColumnNumber - 1).Value = "Всего:";
			worksheet.Cell(rowNumber, dataColumnNumber).Value = ComplaintsDataForDate.Count;
			worksheet.Range(rowNumber, objectNameColumnNumber, rowNumber, dataColumnNumber - 2).Merge();

			FormatComplaintsSummaryTypesAndObjectsTotalCells(
				worksheet.Range(rowNumber, objectNameColumnNumber, rowNumber, dataColumnNumber));
		}

		private void FormatComplaintsWorksheetsTitleCells(IXLRange cellsRange)
		{
			FormatCells(
				cellsRange,
				fontSize: 22,
				isBoldFont: true,
				bgColor: _complaintsTitlesMarkupBgColor,
				horizontalAlignment: XLAlignmentHorizontalValues.Center,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatComplaintsSummaryTypesAndObjectsTotalCells(IXLRange cellsRange)
		{
			FormatCells(
				cellsRange,
				isBoldFont: true,
				bgColor: _complaintsTitlesMarkupBgColor,
				horizontalAlignment: XLAlignmentHorizontalValues.Center,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatComplaintsBoldFontMediumBordersWithBackgroundCells(
			IXLRange cellsRange,
			XLAlignmentHorizontalValues horizontalAlignment = XLAlignmentHorizontalValues.Center)
		{
			FormatCells(
				cellsRange,
				isBoldFont: true,
				bgColor: _complaintsTitlesMarkupBgColor,
				horizontalAlignment: horizontalAlignment,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatComplaintsBoldFontMediumBordersCells(
			IXLRange cellsRange,
			XLAlignmentHorizontalValues horizontalAlignment = XLAlignmentHorizontalValues.Center)
		{
			FormatCells(
				cellsRange,
				isBoldFont: true,
				horizontalAlignment: horizontalAlignment,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatComplaintsDataCommonCells(
			IXLRange cellsRange,
			XLAlignmentHorizontalValues horizontalAlignment = XLAlignmentHorizontalValues.Center)
		{
			FormatCells(
				cellsRange,
				horizontalAlignment: horizontalAlignment,
				cellBorderStyle: XLBorderStyleValues.Thin);
		}
	}
}
