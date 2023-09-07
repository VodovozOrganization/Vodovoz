using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.TrueMark;
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

		public static ProductCodesScanningReport Generate(IUnitOfWork unitOfWork, DateTime createDateFrom, DateTime createDateTo)
		{
			var codesScannedByDrivers = (from productCode in unitOfWork.Session.Query<CashReceiptProductCode>()
										 join cashReceipt in unitOfWork.Session.Query<CashReceipt>() on productCode.CashReceipt.Id equals cashReceipt.Id
										 join trueMarkWaterIdentificationCode in unitOfWork.Session.Query<TrueMarkWaterIdentificationCode>() on productCode.SourceCode.Id equals trueMarkWaterIdentificationCode.Id
										 into trueMarkIdentificationCodes
										 from trueMarkIdentificationCode in trueMarkIdentificationCodes.DefaultIfEmpty()
										 join routeListItem in unitOfWork.Session.Query<RouteListItem>() on cashReceipt.Order.Id equals routeListItem.Order.Id
										 join routeList in unitOfWork.Session.Query<RouteList>() on routeListItem.RouteList.Id equals routeList.Id
										 join driver in unitOfWork.Session.Query<Employee>() on routeList.Driver.Id equals driver.Id
										 where
											 cashReceipt.CreateDate >= createDateFrom
											 && cashReceipt.CreateDate < createDateTo.AddDays(1)
											 && !cashReceipt.WithoutMarks
										 let isProductCodeSingleDuplicated = productCode.IsDuplicateSourceCode
												 && (productCode.DuplicatedIdentificationCodeId == null
													 || unitOfWork.Session.Query<CashReceiptProductCode>()
														 .Where(c => c.DuplicatedIdentificationCodeId == productCode.DuplicatedIdentificationCodeId)
														 .Count() == 1)
										 let isProductCodeMultiplyDuplicated = productCode.IsDuplicateSourceCode
												 && productCode.DuplicatedIdentificationCodeId != null
												 && unitOfWork.Session.Query<CashReceiptProductCode>()
													.Where(c => c.DuplicatedIdentificationCodeId == productCode.DuplicatedIdentificationCodeId)
													.Count() > 1
										 select new ScannedCodeInfo
										 {
											 DriverId = driver.Id,
											 DriverFIO = PersonHelper.PersonNameWithInitials(driver.LastName, driver.Name, driver.Patronymic),
											 SourceCode = productCode.SourceCode,
											 DuplicatedCodeId = productCode.DuplicatedIdentificationCodeId,
											 IsProductCodeSingleDuplicated = isProductCodeSingleDuplicated,
											 IsProductCodeMultiplyDuplicated = isProductCodeMultiplyDuplicated,
											 IsDuplicateSourceCode = productCode.IsDuplicateSourceCode,
											 IsUnscannedSourceCode = productCode.IsUnscannedSourceCode,
											 IsDefectiveSourceCode = productCode.IsDefectiveSourceCode,
											 IsInvalidSourceCode = trueMarkIdentificationCode != null && trueMarkIdentificationCode.IsInvalid
										 }).ToList();

			var groupedByDriverCodes = (from item in codesScannedByDrivers
										group item by new { item.DriverId, item.DriverFIO } into groupedCodes
										select new
										{
											Driver = groupedCodes.Key.DriverFIO,
											ScannedCodes = groupedCodes.ToList()
										}).ToList();

			var rows = new List<Row>();
			var counter = 1;

			foreach(var item in groupedByDriverCodes)
			{
				var driver = item.Driver;
				var codes = item.ScannedCodes;

				var row = new Row();

				row.RowNumber = counter;
				row.DriverFIO = driver;
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
				&& c.SourceCode != null)
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
	}
}
