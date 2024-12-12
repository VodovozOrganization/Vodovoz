using ClosedXML.Excel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.ViewModels.Reports.OKS.DailyReport
{
	public partial class OksDailyReport
	{
		private readonly XLColor _discountsTitlesMarkupBgColor = XLColor.FromColor(Color.FromArgb(220, 197, 225));
		private const int _discountsTableStartColumnNumber = 2;
		private int _discountsNextEmptyRowNumber = 0;

		private IEnumerable<int> _oksDiscountReasonsIds = new List<int>();
		private IEnumerable<int> _productChangeDiscountReasonsIds = new List<int>();
		private IEnumerable<int> _additionalDeliveryDiscountReasonsIds = new List<int>();

		private IList<OksDailyReportOrderDiscountDataNode> _ordersDiscountsDataForDate =
			new List<OksDailyReportOrderDiscountDataNode>();

		private void FillDiscountsWorksheet(ref IXLWorksheet worksheet)
		{
			_discountsNextEmptyRowNumber = 2;

			SetDiscountsColumnsWidth(ref worksheet);
			AddDiscountsTitleTable(ref worksheet);
			AddOksDiscountsTable(ref worksheet);
			AddChangeDiscountsTable(ref worksheet);
			AddAdditionalDeliveryDiscountsTable(ref worksheet);
		}

		private void SetDiscountsColumnsWidth(ref IXLWorksheet worksheet)
		{
			worksheet.Column(1).Width = 4;
			worksheet.Column(2).Width = 8;
			worksheet.Column(3).Width = 44;
			worksheet.Column(4).Width = 10;
			worksheet.Column(5).Width = 16;
			worksheet.Column(6).Width = 16;
			worksheet.Column(7).Width = 16;
			worksheet.Column(8).Width = 16;
			worksheet.Column(9).Width = 48;
			worksheet.Column(10).Width = 16;
		}

		private void AddDiscountsTitleTable(ref IXLWorksheet worksheet)
		{
			var rowNumber = _discountsNextEmptyRowNumber;

			worksheet.Cell(rowNumber, _discountsTableStartColumnNumber).Value = $"Отчет по скидкам {_date.ToString(_dateFormatString)}";

			var cellsRange = worksheet.Range(rowNumber, _discountsTableStartColumnNumber, rowNumber, 10);
			cellsRange.Merge();

			FormatDiscountsWorksheetsTitleCells(cellsRange);

			_discountsNextEmptyRowNumber = rowNumber + 2;
		}

		private void AddOksDiscountsTable(ref IXLWorksheet worksheet)
		{
			worksheet.Cell(_discountsNextEmptyRowNumber, _discountsTableStartColumnNumber).Value = "'1.";
			worksheet.Cell(_discountsNextEmptyRowNumber, _discountsTableStartColumnNumber + 1).Value = "ОКС, ОКС-10%";

			FormatDiscountsBoldFontCells(
				worksheet.Range(_discountsNextEmptyRowNumber, _discountsTableStartColumnNumber, _discountsNextEmptyRowNumber, _discountsTableStartColumnNumber));

			FormatDiscountsBoldFontCells(
				worksheet.Range(_discountsNextEmptyRowNumber, _discountsTableStartColumnNumber + 1, _discountsNextEmptyRowNumber, _discountsTableStartColumnNumber + 1),
				XLAlignmentHorizontalValues.Left);

			_discountsNextEmptyRowNumber++;

			AddDiscountsTablesHeaders(ref worksheet);
			AddDiscountsTablesData(ref worksheet, _oksDiscountReasonsIds);
		}

		private void AddChangeDiscountsTable(ref IXLWorksheet worksheet)
		{
			worksheet.Cell(_discountsNextEmptyRowNumber, _discountsTableStartColumnNumber).Value = "'2.";
			worksheet.Cell(_discountsNextEmptyRowNumber, _discountsTableStartColumnNumber + 1).Value = "Замена";

			FormatDiscountsBoldFontCells(
				worksheet.Range(_discountsNextEmptyRowNumber, _discountsTableStartColumnNumber, _discountsNextEmptyRowNumber, _discountsTableStartColumnNumber));

			FormatDiscountsBoldFontCells(
				worksheet.Range(_discountsNextEmptyRowNumber, _discountsTableStartColumnNumber + 1, _discountsNextEmptyRowNumber, _discountsTableStartColumnNumber + 1),
				XLAlignmentHorizontalValues.Left);

			_discountsNextEmptyRowNumber++;

			AddDiscountsTablesHeaders(ref worksheet);
			AddDiscountsTablesData(ref worksheet, _productChangeDiscountReasonsIds);
		}

		private void AddAdditionalDeliveryDiscountsTable(ref IXLWorksheet worksheet)
		{
			worksheet.Cell(_discountsNextEmptyRowNumber, _discountsTableStartColumnNumber).Value = "'3.";
			worksheet.Cell(_discountsNextEmptyRowNumber, _discountsTableStartColumnNumber + 1).Value = "Довоз";

			FormatDiscountsBoldFontCells(
				worksheet.Range(_discountsNextEmptyRowNumber, _discountsTableStartColumnNumber, _discountsNextEmptyRowNumber, _discountsTableStartColumnNumber));

			FormatDiscountsBoldFontCells(
				worksheet.Range(_discountsNextEmptyRowNumber, _discountsTableStartColumnNumber + 1, _discountsNextEmptyRowNumber, _discountsTableStartColumnNumber + 1),
				XLAlignmentHorizontalValues.Left);

			_discountsNextEmptyRowNumber++;

			AddDiscountsTablesHeaders(ref worksheet);
			AddDiscountsTablesData(ref worksheet, _additionalDeliveryDiscountReasonsIds);
		}

		private void AddDiscountsTablesHeaders(ref IXLWorksheet worksheet)
		{
			var rowNumber = _discountsNextEmptyRowNumber;

			var headersColumnNumber = _discountsTableStartColumnNumber;
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "№";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Клиент";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Заказ №";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Сумма без скидки, руб";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "% Скидки";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Сумма, руб";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Основание скидки";
			worksheet.Cell(rowNumber, headersColumnNumber++).Value = "Товары";
			worksheet.Cell(rowNumber, headersColumnNumber).Value = "Кол-во";

			FormatDiscountsBoldFontMediumBordersWithBackgroundCells(
				worksheet.Range(rowNumber, _discountsTableStartColumnNumber, rowNumber, headersColumnNumber));

			_discountsNextEmptyRowNumber++;
		}

		private void AddDiscountsTablesData(ref IXLWorksheet worksheet, IEnumerable<int> discountReasons)
		{
			var discountsData =
				_ordersDiscountsDataForDate
				.Where(d => discountReasons.Contains(d.DiscountResonId))
				.ToList();

			var rowNumber = _discountsNextEmptyRowNumber;
			var counter = 0;
			var headersColumnNumber = 0;

			foreach(var item in discountsData)
			{
				counter++;
				headersColumnNumber = _discountsTableStartColumnNumber;

				worksheet.Cell(rowNumber, headersColumnNumber++).Value = counter;
				worksheet.Cell(rowNumber, headersColumnNumber++).Value = item.ClientName;
				worksheet.Cell(rowNumber, headersColumnNumber++).Value = item.OrderId;
				worksheet.Cell(rowNumber, headersColumnNumber++).Value = item.OrderItemPrice * item.Amount;
				worksheet.Cell(rowNumber, headersColumnNumber++).Value = item.Discount;
				worksheet.Cell(rowNumber, headersColumnNumber++).Value = item.DiscountMoney;
				worksheet.Cell(rowNumber, headersColumnNumber++).Value = item.DiscountReasonName;
				worksheet.Cell(rowNumber, headersColumnNumber++).Value = item.NomenclatureName;
				worksheet.Cell(rowNumber, headersColumnNumber).Value = item.Amount;

				rowNumber++;
			}

			FormatDiscountsThinBordersCells(
				worksheet.Range(_discountsNextEmptyRowNumber, _discountsTableStartColumnNumber, rowNumber - 1, headersColumnNumber));

			worksheet.Cell(rowNumber, 5).Value = "Итоговая сумма:";
			worksheet.Cell(rowNumber, 7).Value = discountsData.Sum(d => d.DiscountMoney);

			worksheet.Range(rowNumber, 5, rowNumber, 6).Merge();

			FormatDiscountsBoldFontMediumBordersCells(
				worksheet.Range(rowNumber, 5, rowNumber, 7));

			worksheet.Cell(rowNumber, 10).Value = discountsData.Sum(d => d.Amount);

			FormatDiscountsBoldFontMediumBordersCells(
				worksheet.Range(rowNumber, 10, rowNumber, 10));

			rowNumber++;

			_discountsNextEmptyRowNumber = rowNumber + 1;
		}

		private void FormatDiscountsWorksheetsTitleCells(
			IXLRange cellsRange,
			XLAlignmentHorizontalValues horizontalAlignment = XLAlignmentHorizontalValues.Center)
		{
			FormatCells(
				cellsRange,
				fontSize: 16,
				isBoldFont: true,
				bgColor: _discountsTitlesMarkupBgColor,
				horizontalAlignment: horizontalAlignment,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatDiscountsBoldFontCells(
			IXLRange cellsRange,
			XLAlignmentHorizontalValues horizontalAlignment = XLAlignmentHorizontalValues.Center)
		{
			FormatCells(
				cellsRange,
				isBoldFont: true,
				horizontalAlignment: horizontalAlignment);
		}

		private void FormatDiscountsBoldFontMediumBordersWithBackgroundCells(
			IXLRange cellsRange,
			XLAlignmentHorizontalValues horizontalAlignment = XLAlignmentHorizontalValues.Center)
		{
			FormatCells(
				cellsRange,
				isBoldFont: true,
				bgColor: _discountsTitlesMarkupBgColor,
				horizontalAlignment: horizontalAlignment,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatDiscountsBoldFontMediumBordersCells(
			IXLRange cellsRange,
			XLAlignmentHorizontalValues horizontalAlignment = XLAlignmentHorizontalValues.Center)
		{
			FormatCells(
				cellsRange,
				isBoldFont: true,
				horizontalAlignment: horizontalAlignment,
				cellBorderStyle: XLBorderStyleValues.Medium);
		}

		private void FormatDiscountsThinBordersCells(
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
