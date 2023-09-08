using NHibernate;
using NHibernate.SqlCommand;
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
										 join routeListItem in unitOfWork.Session.Query<RouteListItem>() on cashReceipt.Order.Id equals routeListItem.Order.Id
										 join routeList in unitOfWork.Session.Query<RouteList>() on routeListItem.RouteList.Id equals routeList.Id
										 join employee in unitOfWork.Session.Query<Employee>() on routeList.Driver.Id equals employee.Id
										 into drivers
										 from driver in drivers
										 join trueMarkWaterIdentificationCode in unitOfWork.Session.Query<TrueMarkWaterIdentificationCode>() on productCode.SourceCode.Id equals trueMarkWaterIdentificationCode.Id 
										 into trueMarkIdentificationCodes
										 from trueMarkIdentificationCode in trueMarkIdentificationCodes.DefaultIfEmpty()
										 where
											 cashReceipt.CreateDate >= createDateFrom
											 && cashReceipt.CreateDate < createDateTo.AddDays(1)
											 && !cashReceipt.WithoutMarks
										 select new ScannedCodeInfo
										 {
											 DriverId = driver.Id,
											 DriverFIO = PersonHelper.PersonNameWithInitials(driver.LastName, driver.Name, driver.Patronymic),
											 SourceCodeId = productCode.SourceCode.Id,
											 DuplicatedCodeId = productCode.DuplicatedIdentificationCodeId,
											 IsProductCodeSingleDuplicated = productCode.IsDuplicateSourceCode && productCode.DuplicatsCount <= 1,
											 IsProductCodeMultiplyDuplicated = productCode.IsDuplicateSourceCode && productCode.DuplicatsCount > 1,
											 IsDuplicateSourceCode = productCode.IsDuplicateSourceCode,
											 IsUnscannedSourceCode = productCode.IsUnscannedSourceCode,
											 IsDefectiveSourceCode = productCode.IsDefectiveSourceCode,
											 IsInvalidSourceCode = trueMarkIdentificationCode != null && trueMarkIdentificationCode.IsInvalid
										 }).ToList();

			var groupedByDriverCodes = codesScannedByDrivers
				.GroupBy(c => new { c.DriverId, c.DriverFIO })
				.ToDictionary(g => g.Key.DriverFIO, g => g.ToList());

			var rows = new List<Row>();
			var counter = 1;

			foreach(var item in groupedByDriverCodes)
			{
				var driver = item.Key;
				var codes = item.Value;

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
	}
}
