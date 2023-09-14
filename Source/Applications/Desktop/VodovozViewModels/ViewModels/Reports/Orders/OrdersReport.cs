using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using NPOI.SS.Formula.Functions;
using QS.DomainModel.Entity;
using QS.Project.Services.FileDialog;
using System;
using System.Collections.Generic;
using System.Globalization;
using Vodovoz.Domain.Orders;
using Vodovoz.JournalNodes;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.ViewModels.Reports.Orders
{
	[Appellative(Nominative = "Отчет по заказам")]
	public class OrdersReport
	{
		private readonly IFileDialogService _fileDialogService;
		private readonly Report _report;

		#region WorkSheet Config
		private const int _defaultColumnWidth = 12;
		private const int _defaultFontSize = 10;
		private const int _worksheetTitleFontSize = 13;

		private const int _orderIdColumnNumber = 1;
		private const int _dateColumnNumber = 2;
		private const int _authorColumnNumber = 3;
		private const int _deliveryTimeColumnNumber = 4;
		private const int _orderStatusColumnNumber = 5;
		private const int _typeColumnNumber = 6;
		private const int _bottleAmountCountColumnNumber = 7;
		private const int _sanitisationAmountColumnNumber = 8;
		private const int _counterpartyColumnNumber = 9;
		private const int _innColumnNumber = 10;
		private const int _sumColumnNumber = 11;
		private const int _paymentStatusColumnNumber = 12;
		private const int _edoDocFlowStatusColumnNumber = 13;
		private const int _districtNameColumnNumber = 14;
		private const int _addressColumnNumber = 15;
		private const int _lastEditorColumnNumber = 16;
		private const int _lastEditedTimeColumnNumber = 17;
		private const int _driverCallIdColumnNumber = 18;
		private const int _onLineNumberColumnNumber = 19;
		private const int _eShopNumberColumnNumber = 20;
		#endregion Column Config

		public OrdersReport(
			DateTime createDateFrom, 
			DateTime createDateTo, 
			IEnumerable<OrderJournalNode> rows,
			IFileDialogService fileDialogService)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
			_report = new Report(rows);

			CreateDateFrom = createDateFrom;
			CreateDateTo = createDateTo;
			ReportCreatedAt = DateTime.Now;
		}

		public string Title => typeof(OrdersReport).GetClassUserFriendlyName().Nominative;

		public DateTime CreateDateFrom { get; }

		public DateTime CreateDateTo { get; }

		public DateTime ReportCreatedAt { get; }

		public void Export()
		{
			var dialogSettings = new DialogSettings();
			dialogSettings.Title = "Сохранить";
			dialogSettings.DefaultFileExtention = ".xlsx";
			dialogSettings.FileName = $"{Title} {ReportCreatedAt:yyyy-MM-dd-HH-mm}.xlsx";

			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			if(result.Successful)
			{
				SaveReport(result.Path);
			}
		}

		private void SaveReport(string path)
		{
			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Заказы");

				RenderReport(worksheet);

				workbook.SaveAs(path);
			}
		}

		private void RenderReport(IXLWorksheet worksheet)
		{
			var sheetTitleRowNumber = 1;
			var tableTitlesRowNumber = 3;

			SetColumnsWidth(worksheet);

			var reportTitle = $"{Title} за период с {CreateDateFrom:dd.MM.yyyy} по {CreateDateTo:dd.MM.yyyy}";

			RenderWorksheetTitleCell(worksheet, sheetTitleRowNumber, 1, reportTitle);

			RenderTableTitleRow(worksheet, tableTitlesRowNumber);

			var excelRowCounter = ++tableTitlesRowNumber;

			foreach(var row in _report.Rows)
			{
				RenderReportRow(worksheet, excelRowCounter, row);
				excelRowCounter++;
			}
		}

		private void SetColumnsWidth(IXLWorksheet worksheet)
		{
			worksheet.ColumnWidth = _defaultColumnWidth;

			worksheet.Column(_authorColumnNumber).Width = _defaultColumnWidth * 2;
			worksheet.Column(_counterpartyColumnNumber).Width = _defaultColumnWidth * 3;
			worksheet.Column(_districtNameColumnNumber).Width = _defaultColumnWidth * 1.5;
			worksheet.Column(_addressColumnNumber).Width = _defaultColumnWidth * 5;
			worksheet.Column(_lastEditorColumnNumber).Width = _defaultColumnWidth * 2;
			worksheet.Column(_lastEditedTimeColumnNumber).Width = _defaultColumnWidth * 2;
		}

		private void RenderTableTitleRow(IXLWorksheet worksheet, int rowNumber)
		{
			RenderTableTitleCell(worksheet, rowNumber, _orderIdColumnNumber, "Номер");
			RenderTableTitleCell(worksheet, rowNumber, _dateColumnNumber, "Дата");
			RenderTableTitleCell(worksheet, rowNumber, _authorColumnNumber, "Автор");
			RenderTableTitleCell(worksheet, rowNumber, _deliveryTimeColumnNumber, "Время");
			RenderTableTitleCell(worksheet, rowNumber, _orderStatusColumnNumber, "Статус");
			RenderTableTitleCell(worksheet, rowNumber, _typeColumnNumber, "Тип");
			RenderTableTitleCell(worksheet, rowNumber, _bottleAmountCountColumnNumber, "Бутыли");
			RenderTableTitleCell(worksheet, rowNumber, _sanitisationAmountColumnNumber, "Кол-во с/о");
			RenderTableTitleCell(worksheet, rowNumber, _counterpartyColumnNumber, "Клиент");
			RenderTableTitleCell(worksheet, rowNumber, _innColumnNumber, "ИНН");
			RenderTableTitleCell(worksheet, rowNumber, _sumColumnNumber, "Сумма");
			RenderTableTitleCell(worksheet, rowNumber, _paymentStatusColumnNumber, "Статус оплаты");
			RenderTableTitleCell(worksheet, rowNumber, _edoDocFlowStatusColumnNumber, "Статус документооборота");
			RenderTableTitleCell(worksheet, rowNumber, _districtNameColumnNumber, "Район доставки");
			RenderTableTitleCell(worksheet, rowNumber, _addressColumnNumber, "Адрес");
			RenderTableTitleCell(worksheet, rowNumber, _lastEditorColumnNumber, "Изменил");
			RenderTableTitleCell(worksheet, rowNumber, _lastEditedTimeColumnNumber, "Послед. изменения");
			RenderTableTitleCell(worksheet, rowNumber, _driverCallIdColumnNumber, "Номер звонка");
			RenderTableTitleCell(worksheet, rowNumber, _onLineNumberColumnNumber, "Online заказ №");
			RenderTableTitleCell(worksheet, rowNumber, _eShopNumberColumnNumber, "Номер заказа интернет-магазина");
		}

		private void RenderReportRow(IXLWorksheet worksheet, int rowNumber, OrderJournalNode node)
		{

			RenderNumericCell(worksheet, rowNumber, _orderIdColumnNumber, node.Id);
			RenderStringCell(worksheet, rowNumber, _dateColumnNumber, node.Date != null ? ((DateTime)node.Date).ToString("d") : string.Empty);
			RenderStringCell(worksheet, rowNumber, _authorColumnNumber, node.Author);
			RenderStringCell(worksheet, rowNumber, _deliveryTimeColumnNumber, node.IsSelfDelivery ? "-" : node.DeliveryTime);
			RenderStringCell(worksheet, rowNumber, _orderStatusColumnNumber, node.StatusEnum.ToString());
			RenderStringCell(worksheet, rowNumber, _typeColumnNumber, node.ViewType);
			RenderNumericCell(worksheet, rowNumber, _bottleAmountCountColumnNumber, (int)node.BottleAmount);
			RenderNumericCell(worksheet, rowNumber, _sanitisationAmountColumnNumber, (int)node.SanitisationAmount);
			RenderStringCell(worksheet, rowNumber, _counterpartyColumnNumber, node.Counterparty);
			RenderStringCell(worksheet, rowNumber, _innColumnNumber, node.Inn);
			RenderNumericFloatingPointCell(worksheet, rowNumber, _sumColumnNumber, node.Sum);
			RenderStringCell(worksheet, rowNumber, _paymentStatusColumnNumber, ((node.OrderPaymentStatus != OrderPaymentStatus.None) ? node.OrderPaymentStatus.ToString() : ""));
			RenderStringCell(worksheet, rowNumber, _edoDocFlowStatusColumnNumber, node.EdoDocFlowStatus.ToString());
			RenderStringCell(worksheet, rowNumber, _districtNameColumnNumber, node.IsSelfDelivery ? "-" : node.DistrictName);
			RenderStringCell(worksheet, rowNumber, _addressColumnNumber, node.Address);
			RenderStringCell(worksheet, rowNumber, _lastEditorColumnNumber, node.LastEditor);
			RenderStringCell(worksheet, rowNumber, _lastEditedTimeColumnNumber, node.LastEditedTime != default(DateTime) ? node.LastEditedTime.ToString(CultureInfo.CurrentCulture) : string.Empty);
			RenderStringCell(worksheet, rowNumber, _driverCallIdColumnNumber, node.DriverCallId.ToString());
			RenderStringCell(worksheet, rowNumber, _onLineNumberColumnNumber, node.OnLineNumber);
			RenderStringCell(worksheet, rowNumber, _eShopNumberColumnNumber, node.EShopNumber);
		}

		private void RenderWorksheetTitleCell(
			IXLWorksheet worksheet,
			int rowNumber,
			int columnNumber,
			string value)
		{
			RenderCell(worksheet, rowNumber, columnNumber, value, XLDataType.Number, isBold: true, isWrapText: false, fontSize: _worksheetTitleFontSize);
		}

		private void RenderTableTitleCell(
			IXLWorksheet worksheet,
			int rowNumber,
			int columnNumber,
			string value)
		{
			RenderCell(worksheet, rowNumber, columnNumber, value, XLDataType.Number, isBold: true);
		}

		private void RenderNumericCell(
			IXLWorksheet worksheet,
			int rowNumber,
			int columnNumber,
			int value)
		{
			RenderCell(worksheet, rowNumber, columnNumber, value, XLDataType.Number);
		}

		private void RenderNumericFloatingPointCell(
			IXLWorksheet worksheet,
			int rowNumber,
			int columnNumber,
			decimal value)
		{
			RenderCell(worksheet, rowNumber, columnNumber, value, XLDataType.Number, numericFormat: "##0.00");
		}

		private void RenderStringCell(
			IXLWorksheet worksheet,
			int rowNumber,
			int columnNumber,
			string value)
		{
			RenderCell(worksheet, rowNumber, columnNumber, value, XLDataType.Text);
		}

		private void RenderCell(
			IXLWorksheet worksheet,
			int rowNumber,
			int columnNumber,
			object value,
			XLDataType dataType,
			bool isBold = false,
			bool isWrapText = true,
			double? fontSize = null,
			string numericFormat = "")
		{
			var cell = worksheet.Cell(rowNumber, columnNumber);

			cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			cell.Style.Font.Bold = isBold;
			cell.Style.Font.FontSize = fontSize != null ? fontSize.Value : _defaultFontSize;
			cell.Style.Alignment.WrapText = isWrapText;

			cell.DataType = dataType;

			if(dataType == XLDataType.Number)
			{
				if(!string.IsNullOrWhiteSpace(numericFormat))
				{
					cell.Style.NumberFormat.Format = numericFormat;
				}
			}

			cell.Value = value;
		}

		private class Report
		{
			public Report(IEnumerable<OrderJournalNode> rows)
			{
				Rows = rows;
			}

			public IEnumerable<OrderJournalNode> Rows { get; }
		}
	}
}
