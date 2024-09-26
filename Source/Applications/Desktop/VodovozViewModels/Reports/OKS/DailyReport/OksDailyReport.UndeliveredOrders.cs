using ClosedXML.Excel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Undeliveries;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.Reports.OKS.DailyReport
{
	public partial class OksDailyReport
	{
		private readonly XLColor _undeliveredOrdersTitlesMarkupBgColor = XLColor.FromColor(Color.FromArgb(222, 235, 247));
		private readonly XLColor _undeliveredOrdersMarkupBgColor = XLColor.FromColor(Color.FromArgb(146, 208, 80));
		private const int _leftTablesFirstColumn = 2;
		private const int _leftSmallTablesFirstColumn = 3;
		private const int _leftSmallTablesSecondColumn = 4;
		private const int _leftSmallTablesThirdColumn = 5;
		private const int _rightTablesFirstColumn = 7;
		private const int _rightTablesSecondColumn = 8;

		private int _undeliveredNextEmptyRowNumber = 0;

		public IList<OksDailyReportUndeliveredOrderDataNode> UndeliveredOrdersDataForDate { get; private set; } =
			new List<OksDailyReportUndeliveredOrderDataNode>();

		public IList<OksDailyReportUndeliveredOrderDataNode> UndeliveredOrdersDataFromMonthBeginningToDate { get; private set; } =
			new List<OksDailyReportUndeliveredOrderDataNode>();

		private void FillUndeliveredOrdersWorksheet(ref IXLWorksheet worksheet)
		{
			_undeliveredNextEmptyRowNumber = 2;

			SetUndeliveredOrdersColumnsWidth(ref worksheet);
			AddUndeliveredOrdersTitleTable(ref worksheet);
			AddUndeliveredOrdersTotalCountTable(ref worksheet);
			AddUndeliveredOrdersGuiltiesTable(ref worksheet);
			AddUndeliveredOrdersStatusesOnDateTable(ref worksheet);
			AddUndeliveredOrdersStatusesFromMonthStartTable(ref worksheet);
			AddUndeliveredOrdersTransferTypesTable(ref worksheet);
			AddUndeliveredOrdersNoTransferOnClientSummaryTable(ref worksheet);
			AddUndeliveredOrdersNoTransferOnSubdivisionSummaryTable(ref worksheet);
			AddUndeliveredOrdersNoTransferOnClientTable(ref worksheet);
			AddUndeliveredOrdersNoTransferOnSubdivisionTable(ref worksheet);
		}

		private void SetUndeliveredOrdersColumnsWidth(ref IXLWorksheet worksheet)
		{
			worksheet.Column(1).Width = 4;
			worksheet.Column(2).Width = 10;
			worksheet.Column(3).Width = 18;
			worksheet.Column(4).Width = 24;
			worksheet.Column(5).Width = 24;
			worksheet.Column(6).Width = 24;
			worksheet.Column(7).Width = 48;
			worksheet.Column(8).Width = 20;
			worksheet.Column(9).Width = 48;
		}

		private void AddUndeliveredOrdersTitleTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = _undeliveredNextEmptyRowNumber;

			worksheet.Cell(rowNumber, _leftTablesFirstColumn).Value = $"Отчет по недовозам {Date.ToString(_dateFormatString)}";

			var cellsRange = worksheet.Range(rowNumber, _leftTablesFirstColumn, rowNumber, 9);
			cellsRange.Merge();

			FormatUndeliveredOrdersWorksheetsTitleCells(cellsRange);
			_undeliveredNextEmptyRowNumber += 2;
		}

		private void AddUndeliveredOrdersTotalCountTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = _undeliveredNextEmptyRowNumber;

			worksheet.Cell(rowNumber, _leftSmallTablesSecondColumn).Value = $"Всего:";

			worksheet.Cell(rowNumber, _leftSmallTablesThirdColumn).Value =
				UndeliveredOrdersDataForDate
				.Select(uo => uo.UndeliveredOrderId)
				.Distinct()
				.Count();

			FormatUndeliveredOrdersBoldFontMediumBordersWithBackgroundCells(
				worksheet.Range(rowNumber, _leftSmallTablesSecondColumn, rowNumber, _leftSmallTablesSecondColumn));

			FormatUndeliveredOrdersBoldFontMediumBordersCells(
				worksheet.Range(rowNumber, _leftSmallTablesThirdColumn, rowNumber, _leftSmallTablesThirdColumn));

			_undeliveredNextEmptyRowNumber += 2;
		}

		private void AddUndeliveredOrdersGuiltiesTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = _undeliveredNextEmptyRowNumber;

			worksheet.Cell(rowNumber, _leftSmallTablesFirstColumn).Value = $"Виновники:";
			worksheet.Range(rowNumber, _leftSmallTablesFirstColumn, rowNumber, _leftSmallTablesSecondColumn).Merge();

			FormatUndeliveredOrdersBoldFontMediumBordersWithBackgroundCells(
				worksheet.Range(rowNumber, _leftSmallTablesFirstColumn, rowNumber, _leftSmallTablesThirdColumn));

			var guityItems =
				UndeliveredOrdersDataForDate
				.GroupBy(g => new { g.GuiltySide, g.GuiltySubdivisionId })
				.ToDictionary(g => g.Key, g => g.ToList())
				.OrderBy(g => g.Key.GuiltySide);

			foreach(var guiltySideData in guityItems)
			{
				rowNumber++;

				worksheet.Cell(rowNumber, _leftSmallTablesFirstColumn).Value =
					guiltySideData.Key.GuiltySide == Domain.Orders.GuiltyTypes.Department
					? $"Отд: {guiltySideData.Value.FirstOrDefault()?.GuiltySubdivisionName}"
					: guiltySideData.Key.GuiltySide.GetEnumDisplayName();

				worksheet.Range(rowNumber, _leftSmallTablesFirstColumn, rowNumber, _leftSmallTablesSecondColumn).Merge();

				worksheet.Cell(rowNumber, _leftSmallTablesThirdColumn).Value =
					guiltySideData.Value
					.Select(ou => ou.UndeliveredOrderId)
					.Distinct()
					.Count();

				FormatUndeliveredOrdersThinBordersCells(
					worksheet.Range(rowNumber, _leftSmallTablesFirstColumn, rowNumber, _leftSmallTablesThirdColumn));
			}

			_undeliveredNextEmptyRowNumber = rowNumber + 2;
		}

		private void AddUndeliveredOrdersStatusesOnDateTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = _undeliveredNextEmptyRowNumber;

			worksheet.Cell(rowNumber, _leftSmallTablesFirstColumn).Value = $"Статус недовоза:";
			worksheet.Range(rowNumber, _leftSmallTablesFirstColumn, rowNumber, _leftSmallTablesSecondColumn).Merge();

			FormatUndeliveredOrdersBoldFontMediumBordersWithBackgroundCells(
				worksheet.Range(rowNumber, _leftSmallTablesFirstColumn, rowNumber, _leftSmallTablesThirdColumn));

			var statuseItems =
				UndeliveredOrdersDataForDate
				.GroupBy(g => g.UndeliveryStatus)
				.ToDictionary(g => g.Key, g => g.ToList())
				.OrderBy(g => g.Key);

			foreach(var status in statuseItems)
			{
				rowNumber++;

				worksheet.Cell(rowNumber, _leftSmallTablesFirstColumn).Value = status.Key.GetEnumDisplayName();

				worksheet.Range(rowNumber, _leftSmallTablesFirstColumn, rowNumber, _leftSmallTablesThirdColumn).Merge();

				worksheet.Cell(rowNumber, _leftSmallTablesThirdColumn).Value =
					status.Value
					.Select(ou => ou.UndeliveredOrderId)
					.Distinct()
					.Count();

				FormatUndeliveredOrdersThinBordersCells(
					worksheet.Range(rowNumber, _leftSmallTablesFirstColumn, rowNumber, _leftSmallTablesThirdColumn));
			}

			_undeliveredNextEmptyRowNumber = rowNumber + 2;
		}

		private void AddUndeliveredOrdersStatusesFromMonthStartTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = _undeliveredNextEmptyRowNumber;

			worksheet.Cell(rowNumber, _leftSmallTablesFirstColumn).Value = $"Статус недовоза:";
			worksheet.Range(rowNumber, _leftSmallTablesFirstColumn, rowNumber, _leftSmallTablesSecondColumn).Merge();

			FormatUndeliveredOrdersBoldFontMediumBordersWithBackgroundCells(
				worksheet.Range(rowNumber, _leftSmallTablesFirstColumn, rowNumber, _leftSmallTablesThirdColumn));

			var statuseItems =
				UndeliveredOrdersDataFromMonthBeginningToDate
				.GroupBy(g => g.UndeliveryStatus)
				.ToDictionary(g => g.Key, g => g.ToList())
				.OrderBy(g => g.Key);

			foreach(var status in statuseItems)
			{
				rowNumber++;

				worksheet.Cell(rowNumber, _leftSmallTablesFirstColumn).Value = status.Key.GetEnumDisplayName();

				worksheet.Range(rowNumber, _leftSmallTablesFirstColumn, rowNumber, _leftSmallTablesSecondColumn).Merge();

				worksheet.Cell(rowNumber, _leftSmallTablesThirdColumn).Value =
					status.Value
					.Select(ou => ou.UndeliveredOrderId)
					.Distinct()
					.Count();

				FormatUndeliveredOrdersThinBordersCells(
					worksheet.Range(rowNumber, _leftSmallTablesFirstColumn, rowNumber, _leftSmallTablesThirdColumn));
			}

			_undeliveredNextEmptyRowNumber = rowNumber + 2;
		}

		private void AddUndeliveredOrdersTransferTypesTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = 4;

			worksheet.Cell(rowNumber, _rightTablesFirstColumn).Value = $"Вид переноса:";

			FormatUndeliveredOrdersBoldFontMediumBordersWithBackgroundCells(
				worksheet.Range(rowNumber, _rightTablesFirstColumn, rowNumber, _rightTablesSecondColumn));

			var transferTypeItems =
				UndeliveredOrdersDataForDate
				.GroupBy(g => g.TransferType)
				.OrderBy(g => g.Key);

			foreach(var transferType in transferTypeItems)
			{
				rowNumber++;

				worksheet.Cell(rowNumber, _rightTablesFirstColumn).Value =
					transferType.Key.HasValue
					? transferType.Key.Value.GetEnumDisplayName()
					: "Без переноса";

				worksheet.Cell(rowNumber, _rightTablesSecondColumn).Value =
					transferType
					.Select(ou => ou.UndeliveredOrderId)
					.Distinct()
					.Count();

				FormatUndeliveredOrdersThinBordersCells(worksheet.Range(rowNumber, _rightTablesFirstColumn, rowNumber, _rightTablesSecondColumn));
			}
		}

		private void AddUndeliveredOrdersNoTransferOnClientSummaryTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = 11;

			worksheet.Cell(rowNumber, _rightTablesFirstColumn).Value = $"Без переноса на клиенте";

			worksheet.Cell(rowNumber, _rightTablesSecondColumn).Value =
				UndeliveredOrdersDataForDate
				.Where(uo =>
					uo.GuiltySide == GuiltyTypes.Client
					&& uo.NewOrderId is null)
				.Select(ou => ou.UndeliveredOrderId)
				.Distinct()
				.Count();

			FormatUndeliveredOrdersBoldFontMediumBordersWithBackgroundCells(
				worksheet.Range(rowNumber, _rightTablesFirstColumn, rowNumber, _rightTablesFirstColumn));

			FormatUndeliveredOrdersBoldFontMediumBordersCells(
				worksheet.Range(rowNumber, _rightTablesSecondColumn, rowNumber, _rightTablesSecondColumn));
		}

		private void AddUndeliveredOrdersNoTransferOnSubdivisionSummaryTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = 13;

			worksheet.Cell(rowNumber, _rightTablesFirstColumn).Value = $"Без переноса на клиенте";

			worksheet.Cell(rowNumber, _rightTablesSecondColumn).Value =
				UndeliveredOrdersDataForDate
				.Where(uo =>
					uo.GuiltySide == GuiltyTypes.Department
					&& uo.NewOrderId is null)
				.Select(ou => ou.UndeliveredOrderId)
				.Distinct()
				.Count();

			FormatUndeliveredOrdersBoldFontMediumBordersWithBackgroundCells(
				worksheet.Range(rowNumber, _rightTablesFirstColumn, rowNumber, _rightTablesFirstColumn));

			FormatUndeliveredOrdersBoldFontMediumBordersCells(
				worksheet.Range(rowNumber, _rightTablesSecondColumn, rowNumber, _rightTablesSecondColumn));
		}

		private void AddUndeliveredOrdersNoTransferOnClientTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = _undeliveredNextEmptyRowNumber;

			worksheet.Cell(rowNumber, _leftTablesFirstColumn).Value = "Таблица заказов без переноса на клиенте";

			rowNumber++;

			var headersColumnNumber = 2;
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "№";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Статус недовоза";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Дата заказа";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Клиент";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Ответственный";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Причина";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Водитель";
			worksheet.Cell(rowNumber, headersColumnNumber).Value = "Контроль";

			worksheet.Range(_undeliveredNextEmptyRowNumber, _leftTablesFirstColumn, _undeliveredNextEmptyRowNumber, headersColumnNumber).Merge();

			FormatUndeliveredOrdersBoldFontMediumBordersWithBackgroundCells(
				worksheet.Range(_undeliveredNextEmptyRowNumber, _leftTablesFirstColumn, rowNumber, headersColumnNumber));

			var undeliveredOrders =
				UndeliveredOrdersDataForDate
				.Where(uo =>
					uo.GuiltySide == GuiltyTypes.Client
					&& uo.NewOrderId is null)
				.Distinct();

			var counter = 0;

			foreach(var item in undeliveredOrders)
			{
				rowNumber++;
				counter++;

				var columnNumber = _leftTablesFirstColumn;
				worksheet.Cell(rowNumber, columnNumber++).Value = counter;
				worksheet.Cell(rowNumber, columnNumber++).Value = item.UndeliveryStatus.GetEnumDisplayName();
				worksheet.Cell(rowNumber, columnNumber++).Value = item.OldOrderDeliveryDate?.ToString(_dateFormatString);
				worksheet.Cell(rowNumber, columnNumber++).Value = item.ClientName;
				worksheet.Cell(rowNumber, columnNumber++).Value = string.Join(", ", GetGuilties(UndeliveredOrdersDataForDate, item.UndeliveredOrderId));
				worksheet.Cell(rowNumber, columnNumber++).Value = item.Reason;
				worksheet.Cell(rowNumber, columnNumber++).Value = item.DriverNames;
				worksheet.Cell(rowNumber, columnNumber).Value = item.ResultComments;
			}

			FormatUndeliveredOrdersThinBordersCells(worksheet.Range(_undeliveredNextEmptyRowNumber + 2, _leftTablesFirstColumn, rowNumber, headersColumnNumber));

			_undeliveredNextEmptyRowNumber = rowNumber + 2;
		}

		private void AddUndeliveredOrdersNoTransferOnSubdivisionTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = _undeliveredNextEmptyRowNumber;

			worksheet.Cell(rowNumber, _leftTablesFirstColumn).Value = "Таблица заказов без переноса на клиенте";

			rowNumber++;

			var headersColumnNumber = 2;
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "№";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Статус недовоза";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Дата заказа";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Клиент";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Ответственный";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Причина";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Водитель";
			worksheet.Cell(rowNumber, headersColumnNumber).Value = "Контроль";

			worksheet.Range(_undeliveredNextEmptyRowNumber, _leftTablesFirstColumn, _undeliveredNextEmptyRowNumber, headersColumnNumber).Merge();

			FormatUndeliveredOrdersBoldFontMediumBordersWithBackgroundCells(
				worksheet.Range(_undeliveredNextEmptyRowNumber, _leftTablesFirstColumn, rowNumber, headersColumnNumber));

			var undeliveredOrders =
				UndeliveredOrdersDataForDate
				.Where(uo =>
					uo.GuiltySide == GuiltyTypes.Department
					&& uo.NewOrderId is null)
				.Distinct();

			var counter = 0;

			foreach(var item in undeliveredOrders)
			{
				rowNumber++;
				counter++;

				var columnNumber = _leftTablesFirstColumn;
				worksheet.Cell(rowNumber, columnNumber++).Value = counter;
				worksheet.Cell(rowNumber, columnNumber++).Value = item.UndeliveryStatus.GetEnumDisplayName();
				worksheet.Cell(rowNumber, columnNumber++).Value = item.OldOrderDeliveryDate?.ToString(_dateFormatString);
				worksheet.Cell(rowNumber, columnNumber++).Value = item.ClientName;
				worksheet.Cell(rowNumber, columnNumber++).Value = string.Join(", ", GetGuilties(UndeliveredOrdersDataForDate, item.UndeliveredOrderId));
				worksheet.Cell(rowNumber, columnNumber++).Value = item.Reason;
				worksheet.Cell(rowNumber, columnNumber++).Value = item.DriverNames;
				worksheet.Cell(rowNumber, columnNumber).Value = item.ResultComments;
			}

			FormatUndeliveredOrdersThinBordersCells(worksheet.Range(_undeliveredNextEmptyRowNumber + 2, _leftTablesFirstColumn, rowNumber, headersColumnNumber));

			_undeliveredNextEmptyRowNumber = rowNumber + 2;
		}

		private IEnumerable<string> GetGuilties(IEnumerable<OksDailyReportUndeliveredOrderDataNode> undeliveredOrders, int undeliveredOrderId)
		{
			var guilties =
				undeliveredOrders
				.Where(uo => uo.UndeliveredOrderId == undeliveredOrderId)
				.Select(uo => uo.GuiltySide == GuiltyTypes.Department ? $"Отд: {uo.GuiltySubdivisionName}" : uo.GuiltySide.GetEnumDisplayName())
				.Distinct();

			return guilties;
		}

		private void FormatUndeliveredOrdersWorksheetsTitleCells(
			IXLRange cellsRange,
			XLAlignmentHorizontalValues horizontalAlignment = XLAlignmentHorizontalValues.Center)
		{
			FormatCells(
				cellsRange,
				fontSize: 16,
				isBoldFont: true,
				bgColor: _undeliveredOrdersTitlesMarkupBgColor,
				horizontalAlignment: horizontalAlignment,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatUndeliveredOrdersBoldFontMediumBordersWithBackgroundCells(
			IXLRange cellsRange,
			XLAlignmentHorizontalValues horizontalAlignment = XLAlignmentHorizontalValues.Center)
		{
			FormatCells(
				cellsRange,
				isBoldFont: true,
				bgColor: _undeliveredOrdersTitlesMarkupBgColor,
				horizontalAlignment: horizontalAlignment,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatUndeliveredOrdersBoldFontMediumBordersCells(
			IXLRange cellsRange,
			XLAlignmentHorizontalValues horizontalAlignment = XLAlignmentHorizontalValues.Center)
		{
			FormatCells(
				cellsRange,
				isBoldFont: true,
				horizontalAlignment: horizontalAlignment,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatUndeliveredOrdersThinBordersCells(
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
