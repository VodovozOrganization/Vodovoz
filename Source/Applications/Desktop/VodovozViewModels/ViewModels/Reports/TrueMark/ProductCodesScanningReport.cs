using ClosedXML.Excel;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.Extensions;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.ViewModels.Reports.TrueMark
{
	[Appellative(Nominative = "Отчет о сканировании водителями маркировки ЧЗ")]
	public partial class ProductCodesScanningReport
	{
		public ProductCodesScanningReport(DateTime createDateFrom, DateTime createDateTo, IList<Row> rows)
		{
			CreateDateFrom = createDateFrom;
			CreateDateTo = createDateTo;
			Rows = rows;
			ReportCreatedAt = DateTime.Now;
		}

		public string Title => typeof(ProductCodesScanningReport).GetClassUserFriendlyName().Nominative;

		public DateTime CreateDateFrom { get; }

		public DateTime CreateDateTo { get; }

		public DateTime ReportCreatedAt { get; }

		public IList<Row> Rows { get; set; }

		public static async Task<ProductCodesScanningReport> GenerateAsync(IUnitOfWork unitOfWork, DateTime createDateFrom, DateTime createDateTo)
		{
			var report = await Task.Run(() => Generate(unitOfWork, createDateFrom, createDateTo));
			return report;
		}

		public static ProductCodesScanningReport Generate(IUnitOfWork unitOfWork, DateTime createDateFrom, DateTime createDateTo)
		{
			var scannedCodesDataQuery =
				from routeList in unitOfWork.Session.Query<RouteList>()
				join routeListItem in unitOfWork.Session.Query<RouteListItem>() on routeList.Id equals routeListItem.RouteList.Id
				join order in unitOfWork.Session.Query<Domain.Orders.Order>() on routeListItem.Order.Id equals order.Id
				join client in unitOfWork.Session.Query<Domain.Client.Counterparty>() on order.Client.Id equals client.Id
				join driver in unitOfWork.Session.Query<Employee>() on routeList.Driver.Id equals driver.Id
				join sd in unitOfWork.Session.Query<Subdivision>() on driver.Subdivision.Id equals sd.Id into subdivisions
				from subdivision in subdivisions.DefaultIfEmpty()
				join gg in unitOfWork.Session.Query<GeoGroup>() on subdivision.GeographicGroup.Id equals gg.Id into geoGroups
				from geoGroup in geoGroups.DefaultIfEmpty()
				join sc in unitOfWork.Session.Query<RouteListItemTrueMarkProductCode>() on routeListItem.Id equals sc.RouteListItem.Id into scannedCodes
				from scannedCode in scannedCodes.DefaultIfEmpty()
				join ic in unitOfWork.Session.Query<TrueMarkWaterIdentificationCode>() on scannedCode.SourceCode.Id equals ic.Id into identificationCodes
				from identificationCode in identificationCodes.DefaultIfEmpty()

				let markedProductsInOrderCount =
					(int?)(from orderItem in unitOfWork.Session.Query<OrderItem>()
						   join Nomenclature in unitOfWork.Session.Query<Nomenclature>() on orderItem.Nomenclature.Id equals Nomenclature.Id
						   where
							orderItem.Order.Id == order.Id
							&& Nomenclature.IsAccountableInTrueMark
						   select orderItem.ActualCount)
						   .Sum() ?? 0

				where
					routeList.Date >= createDateFrom && routeList.Date < createDateTo.AddDays(1)
					&& routeListItem.Status == RouteListItemStatus.Completed
					&& !(order.PaymentType == Domain.Client.PaymentType.Cashless
						&& client.ConsentForEdoStatus == ConsentForEdoStatus.Agree
						&& client.OrderStatusForSendingUpd == OrderStatusForSendingUpd.EnRoute)
					&& markedProductsInOrderCount > 0

				select new ScannedCodeInfo
				{
					DriverId = driver.Id,
					DriverFIO = driver.FullName,
					CarOwnType = driver.DriverOfCarOwnType,
					DriverSubdivisionGeoGroup = geoGroup.Name,
					OrderId = order.Id,
					MarkedProdictsInOrderCount = markedProductsInOrderCount,
					ScannedCodeData = new ScannedCodeData
					{
						Problem = scannedCode == null ? ProductCodeProblem.Unscanned : scannedCode.Problem,
						DuplicatesCount = scannedCode == null ? default : scannedCode.DuplicatesCount,
						IsInvalid = identificationCode == null ? default : identificationCode.IsInvalid,
						UnscannedCodesCount = scannedCode == null ? markedProductsInOrderCount : scannedCode.Problem == ProductCodeProblem.Unscanned ? 1 : 0
					}
				};

			var scannedCodesData = scannedCodesDataQuery.ToList();


			var groupedByDriverCodesData = scannedCodesData
				.GroupBy(c => new { c.DriverId, c.DriverFIO, c.CarOwnType, c.DriverSubdivisionGeoGroup })
				.ToDictionary(
					g => g.Key,
					g => g.Select(x => new { x.OrderId, x.MarkedProdictsInOrderCount, x.ScannedCodeData }).ToList());

			var rows = new List<Row>();
			var counter = 1;

			foreach(var driverCodesData in groupedByDriverCodesData)
			{
				var driver = driverCodesData.Key.DriverFIO;
				var carOwnType = driverCodesData.Key.CarOwnType;
				var driverSubdivisionGeoGroup = driverCodesData.Key.DriverSubdivisionGeoGroup;

				var allCodes = driverCodesData.Value
					.Select(x => x.ScannedCodeData)
					.ToList();

				var codesByOrders = driverCodesData.Value
					.GroupBy(x => new { x.OrderId, x.MarkedProdictsInOrderCount })
					.ToDictionary(g => g.Key, g => g.ToList());

				var row = new Row();

				row.DriverFIO = driver;
				row.CarOwnTypeString = carOwnType == null ? string.Empty : carOwnType.Value.GetEnumDisplayName();
				row.DriverSubdivisionGeoGroup = driverSubdivisionGeoGroup;
				row.TotalCodesCount = codesByOrders.Select(x => x.Key.MarkedProdictsInOrderCount).Sum();
				row.SuccessfullyScannedCodesCount = allCodes.Count(x => x.Problem == ProductCodeProblem.None && !x.IsInvalid);
				row.UnscannedCodesCount = allCodes.Where(x => x.Problem == ProductCodeProblem.Unscanned && !x.IsInvalid).Sum(x => x.UnscannedCodesCount);
				row.SingleDuplicatedCodesCount = allCodes.Count(x => x.Problem == ProductCodeProblem.Duplicate && x.DuplicatesCount <= 1 && !x.IsInvalid);
				row.MultiplyDuplicatedCodesCount = allCodes.Count(x => x.Problem == ProductCodeProblem.Duplicate && x.DuplicatesCount > 1 && !x.IsInvalid);
				row.InvalidCodesCount = allCodes.Count(x => x.IsInvalid);

				rows.Add(row);
				counter++;
			}

			rows = rows.OrderBy(r => r.SuccessfullyScannedCodesPercent).ToList();
			return new ProductCodesScanningReport(createDateFrom, createDateTo, rows);
		}

		#region Export report to Excel

		public async Task ExportReportToExcelAsync(string path)
		{
			await Task.Run(() => ExportReportToExcel(path));
		}

		private void ExportReportToExcel(string path)
		{
			using(var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("Сканирование кодов маркировки");
				var sheetTitleRowNumber = 1;
				var tableTitlesRowNumber = 3;

				SetColumnsWidth(worksheet);

				var reportTitle = $"{Title} за период с {CreateDateFrom:dd.MM.yyyy} по {CreateDateTo:dd.MM.yyyy}";

				RenderWorksheetTitleCell(worksheet, sheetTitleRowNumber, 1, reportTitle);

				RenderTableTitleRow(worksheet, tableTitlesRowNumber);

				var excelRowCounter = ++tableTitlesRowNumber;

				for(int i = 0; i < Rows.Count; i++)
				{
					RenderReportRow(worksheet, excelRowCounter, Rows[i], i + 1);
					excelRowCounter++;
				}

				workbook.SaveAs(path);
			}
		}

		private void SetColumnsWidth(IXLWorksheet worksheet)
		{
			var firstColumnWidth = 5;
			var columnsWidth = 18;

			for(int i = 0; i < 13; i++)
			{
				var column = worksheet.Column(i + 1);

				column.Width = i == 0 ? firstColumnWidth : columnsWidth;
			}
		}

		private void RenderTableTitleRow(IXLWorksheet worksheet, int rowNumber)
		{
			var colNumber = 1;

			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "№");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Водитель");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Принадлежность ТС");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Площадка");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Требуется кодов в заказах за период, шт.");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Успешно отсканировано кодов, шт.");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Успешно отсканировано кодов, %");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Не отсканировано кодов, шт.");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Не отсканировано кодов, %");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Дубликаты одноразовые (из пула), шт.");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Дубликаты одноразовые (из пула), %");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Дубликаты множественные, шт.");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Дубликаты множественные, %");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Недействительные коды, шт.");
			RenderTableTitleCell(worksheet, rowNumber, colNumber++, "Недействительные коды, %");
		}

		private void RenderReportRow(IXLWorksheet worksheet, int rowNumber, ProductCodesScanningReport.Row values, int dataNumber)
		{
			var colNumber = 1;

			RenderNumericCell(worksheet, rowNumber, colNumber++, dataNumber);
			RenderStringCell(worksheet, rowNumber, colNumber++, values.DriverFIO);
			RenderStringCell(worksheet, rowNumber, colNumber++, values.CarOwnTypeString);
			RenderStringCell(worksheet, rowNumber, colNumber++, values.DriverSubdivisionGeoGroup);
			RenderNumericCell(worksheet, rowNumber, colNumber++, values.TotalCodesCount);
			RenderNumericCell(worksheet, rowNumber, colNumber++, values.SuccessfullyScannedCodesCount);
			RenderNumericFloatingPointCell(worksheet, rowNumber, colNumber++, values.SuccessfullyScannedCodesPercent);
			RenderNumericCell(worksheet, rowNumber, colNumber++, values.UnscannedCodesCount);
			RenderNumericFloatingPointCell(worksheet, rowNumber, colNumber++, values.UnscannedCodesPercent);
			RenderNumericCell(worksheet, rowNumber, colNumber++, values.SingleDuplicatedCodesCount);
			RenderNumericFloatingPointCell(worksheet, rowNumber, colNumber++, values.SingleDuplicatedCodesPercent);
			RenderNumericCell(worksheet, rowNumber, colNumber++, values.MultiplyDuplicatedCodesCount);
			RenderNumericFloatingPointCell(worksheet, rowNumber, colNumber++, values.MultiplyDuplicatedCodesPercent);
			RenderNumericCell(worksheet, rowNumber, colNumber++, values.InvalidCodesCount);
			RenderNumericFloatingPointCell(worksheet, rowNumber, colNumber++, values.InvalidCodesPercent);
		}

		private void RenderWorksheetTitleCell(
			IXLWorksheet worksheet,
			int rowNumber,
			int columnNumber,
			string value)
		{
			RenderCell(worksheet, rowNumber, columnNumber, value, XLDataType.Number, isBold: true, isWrapText: false, fontSize: 13);
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
			double fontSize = 11,
			string numericFormat = "")
		{
			var cell = worksheet.Cell(rowNumber, columnNumber);

			cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
			cell.Style.Font.Bold = isBold;
			cell.Style.Font.FontSize = fontSize;
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
		#endregion
	}
}
