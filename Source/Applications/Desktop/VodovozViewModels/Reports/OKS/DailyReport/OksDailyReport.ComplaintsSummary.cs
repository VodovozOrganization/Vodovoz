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
		private const int _complaintsSummaryTableStartColumnNumber = 2;
		private int _complaintsSummaryNextEmptyRowNumber = 0;
		private int _incomingCallSourseId;
		private int _oksSubdivisionId;
		private readonly XLColor _complaintsTitlesMarkupBgColor = XLColor.FromColor(Color.FromArgb(226, 240, 217));

		private IList<OksDailyReportComplaintDataNode> _complaintsDataOnDate =
			new List<OksDailyReportComplaintDataNode>();
		private IList<OksDailyReportComplaintDataNode> _complaintsDataFromMonthBeginningToDate =
			new List<OksDailyReportComplaintDataNode>();

		private IList<DiscussionSubdivisionData> _oksDiscussionsDataOnDate =>
			_complaintsDataOnDate
			.SelectMany(c => c.DiscussionSubdivisions)
			.Where(d => d.SubdivisionId == _oksSubdivisionId)
			.Distinct()
			.ToList();

		private IList<DiscussionSubdivisionData> _oksDiscussionsDataFromMonthBeginningToDate =>
			_complaintsDataFromMonthBeginningToDate
			.SelectMany(c => c.DiscussionSubdivisions)
			.Where(d => d.SubdivisionId == _oksSubdivisionId)
			.Distinct()
			.ToList();

		private void FillComplaintsSummaryWorksheet(ref IXLWorksheet worksheet)
		{
			_complaintsSummaryNextEmptyRowNumber = 2;

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
			var rowNumber = _complaintsSummaryNextEmptyRowNumber;

			worksheet.Cell(rowNumber, _complaintsSummaryTableStartColumnNumber).Value = $"Отчет по рекламациям ОКС {_date.ToString(_dateFormatString)}";

			var cellsRange = worksheet.Range(rowNumber, _complaintsSummaryTableStartColumnNumber, rowNumber, 14);
			cellsRange.Merge();

			FormatComplaintsWorksheetsTitleCells(cellsRange);

			_complaintsSummaryNextEmptyRowNumber += 2;
		}

		private void AddComplaintsSummaryByComplaintSourceTable(ref IXLWorksheet worksheet)
		{
			var startRowNumber = _complaintsSummaryNextEmptyRowNumber;
			var labelColumnNumber = _complaintsSummaryTableStartColumnNumber;
			var dataColumnNumber = labelColumnNumber + 1;
			var rowNumber = startRowNumber;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = "Всего рекламаций за смену:";
			worksheet.Cell(rowNumber, dataColumnNumber).Value = _complaintsDataOnDate.Count;

			rowNumber++;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = " - Входящие звонки";
			worksheet.Cell(rowNumber, dataColumnNumber).Value = _complaintsDataOnDate.Where(c => c.ComplaintSource.Id == _incomingCallSourseId).Count();

			rowNumber++;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = " - Чат \"Обращения\"";
			worksheet.Cell(rowNumber, dataColumnNumber).Value = _complaintsDataOnDate.Where(c => c.ComplaintSource.Id != _incomingCallSourseId).Count();

			FormatComplaintsBoldFontMediumBordersWithBackgroundCells(
				worksheet.Range(startRowNumber, labelColumnNumber, rowNumber, labelColumnNumber),
				XLAlignmentHorizontalValues.Left);

			FormatComplaintsBoldFontMediumBordersCells(
				worksheet.Range(startRowNumber, dataColumnNumber, rowNumber, dataColumnNumber));

			_complaintsSummaryNextEmptyRowNumber = rowNumber + 2;
		}

		private void AddComplaintsSummaryByComplaintResultTable(ref IXLWorksheet worksheet)
		{
			var startRowNumber = 4;
			var labelColumnNumber = _complaintsSummaryTableStartColumnNumber + 3;
			var dataColumnNumber = labelColumnNumber + 1;
			var rowNumber = startRowNumber;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = "Проблема решена";
			worksheet.Cell(rowNumber, dataColumnNumber).Value =
				_complaintsDataOnDate
				.Where(c => c.WorkWithClientResult == VodovozBusiness.Domain.Complaints.ComplaintWorkWithClientResult.Solved)
				.Count();

			rowNumber++;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = "Проблема НЕ решена";
			worksheet.Cell(rowNumber, dataColumnNumber).Value =
				_complaintsDataOnDate
				.Where(c => c.WorkWithClientResult == VodovozBusiness.Domain.Complaints.ComplaintWorkWithClientResult.NotSolved)
				.Count();

			rowNumber++;

			worksheet.Cell(rowNumber, labelColumnNumber).Value = "Результат не указан";
			worksheet.Cell(rowNumber, dataColumnNumber).Value =
				_complaintsDataOnDate
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
			var firstColumnNumber = _complaintsSummaryTableStartColumnNumber + 6;
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
				_oksDiscussionsDataOnDate
				.Where(d => d.DiscussionStatuse == ComplaintDiscussionStatuses.InProcess)
				.Select(d => d.ComplaintId)
				.Distinct()
				.Count();

			worksheet.Cell(rowNumber, secondColumnNumber).Value =
				_oksDiscussionsDataOnDate
				.Where(d => d.DiscussionStatuse == ComplaintDiscussionStatuses.Checking)
				.Select(d => d.ComplaintId)
				.Distinct()
				.Count();

			worksheet.Cell(rowNumber, thirdColumnNumber).Value =
				_oksDiscussionsDataOnDate
				.Where(d => d.DiscussionStatuse == ComplaintDiscussionStatuses.Closed)
				.Select(d => d.ComplaintId)
				.Distinct()
				.Count();

			var dataCellsRange = worksheet.Range(rowNumber, firstColumnNumber, rowNumber, thirdColumnNumber);
			FormatComplaintsBoldFontMediumBordersCells(dataCellsRange);
		}

		private void AddComplaintsSummaryByComplaintStatusFromMonthStartTable(ref IXLWorksheet worksheet)
		{
			var startRowNumber = 4;
			var firstColumnNumber = _complaintsSummaryTableStartColumnNumber + 10;
			var secondColumnNumber = firstColumnNumber + 1;
			var thirdColumnNumber = firstColumnNumber + 2;
			var rowNumber = startRowNumber;

			worksheet.Cell(rowNumber, firstColumnNumber).Value =
				$"Статус рекламаций с {_date.FirstDayOfMonth().ToString(_dateFormatString)} по {_date.ToString(_dateFormatString)}";

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
				_oksDiscussionsDataFromMonthBeginningToDate
				.Where(d => d.DiscussionStatuse == ComplaintDiscussionStatuses.InProcess)
				.Select(d => d.ComplaintId)
				.Distinct()
				.Count();

			worksheet.Cell(rowNumber, secondColumnNumber).Value =
				_oksDiscussionsDataFromMonthBeginningToDate
				.Where(d => d.DiscussionStatuse == ComplaintDiscussionStatuses.Checking)
				.Select(d => d.ComplaintId)
				.Distinct()
				.Count();

			worksheet.Cell(rowNumber, thirdColumnNumber).Value =
				_oksDiscussionsDataFromMonthBeginningToDate
				.Where(d => d.DiscussionStatuse == ComplaintDiscussionStatuses.Closed)
				.Select(d => d.ComplaintId)
				.Distinct()
				.Count();

			var dataCellsRange = worksheet.Range(rowNumber, firstColumnNumber, rowNumber, thirdColumnNumber);
			FormatComplaintsBoldFontMediumBordersCells(dataCellsRange);
		}

		private void AddComplaintsSummaryTypesAndObjectsTable(ref IXLWorksheet worksheet)
		{
			var startRowNumber = _complaintsSummaryNextEmptyRowNumber;
			var objectNameColumnNumber = _complaintsSummaryTableStartColumnNumber;
			var typeNameColumnNumber = _complaintsSummaryTableStartColumnNumber + 2;
			var dataColumnNumber = typeNameColumnNumber + 6;
			var rowNumber = startRowNumber;

			worksheet.Cell(rowNumber, objectNameColumnNumber).Value = "Виды и объекты рекламаций";

			var headerCellsRange = worksheet.Range(rowNumber, objectNameColumnNumber, rowNumber, dataColumnNumber);
			headerCellsRange.Merge();
			FormatComplaintsBoldFontMediumBordersWithBackgroundCells(headerCellsRange);

			rowNumber++;

			var complaintsHavingObjects = _complaintsDataOnDate
				.Where(c => c.ComplaintObject != null);

			var groupedByObjectItems = complaintsHavingObjects
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
			worksheet.Cell(rowNumber, dataColumnNumber).Value = complaintsHavingObjects.Count();
			worksheet.Range(rowNumber, objectNameColumnNumber, rowNumber, dataColumnNumber - 2).Merge();

			FormatComplaintsSummaryTypesAndObjectsTotalCells(
				worksheet.Range(rowNumber, objectNameColumnNumber, rowNumber, dataColumnNumber));

			_complaintsSummaryNextEmptyRowNumber = rowNumber + 2;
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
