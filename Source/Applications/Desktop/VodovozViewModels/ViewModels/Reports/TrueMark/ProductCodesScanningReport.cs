using ClosedXML.Excel;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Project.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;
using Vodovoz.Domain.TrueMark;
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
			CashReceipt cashReceiptAlias = null;
			CashReceiptProductCode cashReceiptProductCodeAlias = null;
			RouteListItem routeListItemAlias = null;
			RouteList routeListAlias = null;
			Employee driverAlias = null;
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCodeAlias = null;
			CarVersion carVersionAlias = null;
			Subdivision subdivisionAlias = null;
			GeoGroup geoGroupAlias = null;
			ScannedCodeInfo resultAlias = null;

			var driverFioProjection = CustomProjections.Concat_WS(
				" ",
				Projections.Property(() => driverAlias.LastName),
				Projections.Property(() => driverAlias.Name),
				Projections.Property(() => driverAlias.Patronymic)
			);

			var isProductCodeSingleDuplicatedProjection = Projections.Conditional(
				Restrictions.Conjunction()
					.Add(() => cashReceiptProductCodeAlias.IsDuplicateSourceCode)
					.Add(() => cashReceiptProductCodeAlias.DuplicatesCount <= 1),
				Projections.Constant(true),
				Projections.Constant(false)
				);

			var isProductCodeMultiplyDuplicatedProjection = Projections.Conditional(
				Restrictions.Conjunction()
					.Add(() => cashReceiptProductCodeAlias.IsDuplicateSourceCode)
					.Add(() => cashReceiptProductCodeAlias.DuplicatesCount > 1),
				Projections.Constant(true),
				Projections.Constant(false)
				);

			var isInvalidSourceCodeProjection = Projections.Conditional(
				Restrictions.Conjunction()
					.Add(() => trueMarkWaterIdentificationCodeAlias != null)
					.Add(() => trueMarkWaterIdentificationCodeAlias.IsInvalid),
				Projections.Constant(true),
				Projections.Constant(false)
				);

			var codesScannedByDrivers = unitOfWork.Session.QueryOver(() => cashReceiptAlias)
				.JoinEntityAlias(() => cashReceiptProductCodeAlias, () => cashReceiptProductCodeAlias.CashReceipt.Id == cashReceiptAlias.Id, JoinType.InnerJoin)
				.Left.JoinAlias(() => cashReceiptProductCodeAlias.SourceCode, () => trueMarkWaterIdentificationCodeAlias)
				.JoinEntityAlias(() => routeListItemAlias, () => cashReceiptAlias.Order.Id == routeListItemAlias.Order.Id, JoinType.InnerJoin)
				.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
				.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				//.JoinEntityAlias(
				//		() => carVersionAlias,
				//		() => carVersionAlias.Car.Id == routeListAlias.Car.Id
				//			&& carVersionAlias.StartDate <= routeListAlias.Date
				//			&& (carVersionAlias.EndDate == null || carVersionAlias.EndDate > routeListAlias.Date),
				//		JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => driverAlias.Subdivision, () => subdivisionAlias)
				.Left.JoinAlias(() => subdivisionAlias.GeographicGroup, () => geoGroupAlias)
				.Where(() => cashReceiptAlias.CreateDate >= createDateFrom
					&& cashReceiptAlias.CreateDate < createDateTo.AddDays(1)
					&& !cashReceiptAlias.WithoutMarks)
				.SelectList(list => list
					.Select(() => driverAlias.Id).WithAlias(() => resultAlias.DriverId)
					.Select(driverFioProjection).WithAlias(() => resultAlias.DriverFIO)
					//.Select(() => carVersionAlias.CarOwnType).WithAlias(() => resultAlias.CarOwnType)
					.Select(() => driverAlias.DriverOfCarOwnType).WithAlias(() => resultAlias.CarOwnType)
					.Select(() => geoGroupAlias.Name).WithAlias(() => resultAlias.DriverSubdivisionGeoGroup)
					.Select(() => cashReceiptProductCodeAlias.SourceCode.Id).WithAlias(() => resultAlias.SourceCodeId)
					.Select(() => cashReceiptProductCodeAlias.IsDuplicateSourceCode).WithAlias(() => resultAlias.IsDuplicateSourceCode)
					.Select(isProductCodeSingleDuplicatedProjection).WithAlias(() => resultAlias.IsProductCodeSingleDuplicated)
					.Select(isProductCodeMultiplyDuplicatedProjection).WithAlias(() => resultAlias.IsProductCodeMultiplyDuplicated)
					.Select(() => cashReceiptProductCodeAlias.IsUnscannedSourceCode).WithAlias(() => resultAlias.IsUnscannedSourceCode)
					.Select(() => cashReceiptProductCodeAlias.IsDefectiveSourceCode).WithAlias(() => resultAlias.IsDefectiveSourceCode)
					.Select(() => trueMarkWaterIdentificationCodeAlias.IsInvalid).WithAlias(() => resultAlias.IsInvalidSourceCode))
				.TransformUsing(Transformers.AliasToBean<ScannedCodeInfo>())
				.List<ScannedCodeInfo>()
				.ToList();

			var groupedByDriverCodes = codesScannedByDrivers
				.GroupBy(c => new { c.DriverId, c.DriverFIO, c.CarOwnType, c.DriverSubdivisionGeoGroup })
				.ToDictionary(g => g.Key, g => g.ToList());

			var rows = new List<Row>();
			var counter = 1;

			foreach(var item in groupedByDriverCodes)
			{
				var driver = item.Key.DriverFIO;
				var carOwnType = item.Key.CarOwnType;
				var driverSubdivisionGeoGroup = item.Key.DriverSubdivisionGeoGroup;
				var codes = item.Value;

				var row = new Row();

				row.DriverFIO = driver;
				row.CarOwnTypeString = carOwnType.GetEnumDisplayName();
				row.DriverSubdivisionGeoGroup = driverSubdivisionGeoGroup;
				row.TotalCodesCount = codes.Count;
				row.SuccessfullyScannedCodesCount = GetSuccessfullyScannedCodesCount(codes);
				row.UnscannedCodesCount = GetUnscannedCodesCount(codes);
				row.SingleDuplicatedCodesCount = GetSingleDuplicatedCodesCount(codes);
				row.MultiplyDuplicatedCodesCount = GetMultiplyDuplicatedCodesCount(codes);
				row.InvalidCodesCount = GetInvalidCodesCount(codes);

				rows.Add(row);
				counter++;
			}

			rows = rows.OrderBy(r => r.SuccessfullyScannedCodesPercent).ToList();
			return new ProductCodesScanningReport(createDateFrom, createDateTo, rows);
		}

		private static int GetSuccessfullyScannedCodesCount(List<ScannedCodeInfo> codes) =>
			codes
			.Where(c => !c.IsDuplicateSourceCode
				&& !c.IsUnscannedSourceCode
				&& !c.IsInvalidSourceCode
				&& c.SourceCodeId != null)
			.Count();

		private static int GetUnscannedCodesCount(List<ScannedCodeInfo> codes) =>
			codes
			.Where(c => c.IsUnscannedSourceCode)
			.Count();

		private static int GetSingleDuplicatedCodesCount(List<ScannedCodeInfo> codes) =>
			codes
			.Where(c => c.IsProductCodeSingleDuplicated && !c.IsInvalidSourceCode)
			.Count();

		private static int GetMultiplyDuplicatedCodesCount(List<ScannedCodeInfo> codes) =>
			codes
			.Where(c => c.IsProductCodeMultiplyDuplicated && !c.IsInvalidSourceCode)
			.Count();

		private static int GetInvalidCodesCount(List<ScannedCodeInfo> codes) =>
			codes
			.Where(c => c.IsInvalidSourceCode)
			.Count();

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
