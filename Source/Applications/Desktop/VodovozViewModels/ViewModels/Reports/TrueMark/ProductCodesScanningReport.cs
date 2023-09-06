using DocumentFormat.OpenXml.Validation;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.TrueMark;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.ViewModels.Reports.TrueMark
{
	[Appellative(Nominative = "Отчет о сканировании водителями маркировки ЧЗ")]
	public class ProductCodesScanningReport
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
			var codesScannedByDrivers = from productCode in unitOfWork.Session.Query<CashReceiptProductCode>()
										join cashReceipt in unitOfWork.Session.Query<CashReceipt>() on productCode.CashReceipt.Id equals cashReceipt.Id
										join trueMarkWaterIdentificationCode in unitOfWork.Session.Query<TrueMarkWaterIdentificationCode>() on productCode.SourceCode.Id equals trueMarkWaterIdentificationCode.Id
										into trueMarkIdentificationCodes
										from trueMarkIdentificationCode in trueMarkIdentificationCodes.DefaultIfEmpty()
										join routeListItem in unitOfWork.Session.Query<RouteListItem>() on cashReceipt.Order.Id equals routeListItem.Order.Id
										join routeList in unitOfWork.Session.Query<RouteList>() on routeListItem.RouteList.Id equals routeList.Id
										join driver in unitOfWork.Session.Query<Employee>() on routeList.Driver.Id equals driver.Id
										where
											cashReceipt.CreateDate >= createDateFrom
											&& cashReceipt.CreateDate <= createDateTo
											&& !cashReceipt.WithoutMarks

										let isProductCodeSingleDuplicated = productCode.IsDuplicateSourceCode
											&& (productCode.DuplicatedIdentificationCodeId == null
												|| unitOfWork.Session.Query<CashReceiptProductCode>()
													.Where(c => c.DuplicatedIdentificationCodeId == productCode.DuplicatedIdentificationCodeId)
													.Count() == 1)

										let isProductCodeMultiplyDuplicated = productCode.IsDuplicateSourceCode
											&& (productCode.DuplicatedIdentificationCodeId == null
												|| unitOfWork.Session.Query<CashReceiptProductCode>()
													.Where(c => c.DuplicatedIdentificationCodeId == productCode.DuplicatedIdentificationCodeId)
													.Count() > 1)

										select new
										{
											Driver = driver,
											ProductCodeInfo = new
											{
												ProductCodeId = productCode.Id,
												IsDuplicateSourceCode = productCode.IsDuplicateSourceCode,
												IsUnscannedSourceCode = productCode.IsUnscannedSourceCode,
												IsDefectiveSourceCode = productCode.IsDefectiveSourceCode,
												SourceCode = productCode.SourceCode,
												DuplicatedCodeId = productCode.DuplicatedIdentificationCodeId,
												IsProductCodeSingleDuplicated = isProductCodeSingleDuplicated,
												IsProductCodeMultiplyDuplicated = isProductCodeMultiplyDuplicated,
												ProductCodeIsInvalid = trueMarkIdentificationCode != null && trueMarkIdentificationCode.IsInvalid
											}
										};

			var q = codesScannedByDrivers.ToList();

			var groupedByDriverCodes = from item in codesScannedByDrivers
									   group item.ProductCodeInfo by item.Driver into groupedCodes
									   select new
									   {
										   Driver = groupedCodes.Key,
										   ProductCodes = groupedCodes.ToList()
									   };

			var rows = new List<Row>();
			var counter = 1;

			foreach(var item in groupedByDriverCodes)
			{
				var driver = item.Driver;
				var codes = item.ProductCodes;

				var row = new Row();

				row.RowNumber = counter;
				row.DriverFIO = PersonHelper.PersonNameWithInitials(driver.LastName, driver.Name, driver.Patronymic);
				row.TotalCodesCount = codes.Count;

				row.SuccessfullyScannedCodesCount = codes
					.Where(c =>
						!c.IsDuplicateSourceCode
						&& !c.IsUnscannedSourceCode
						&& c.SourceCode != null)
					.Count();

				row.UnscannedCodesCount = codes
					.Where(c => c.IsUnscannedSourceCode)
					.Count();

				row.SingleDuplicatedCodesCount = codes
					.Where(c => c.IsProductCodeSingleDuplicated)
					.Count();

				row.MultiplyDuplicatedCodesCount = codes
					.Where(c => c.IsProductCodeMultiplyDuplicated)
					.Count();

				row.InvalidCodesCount = codes
					.Where(c => c.ProductCodeIsInvalid)
					.Count();

				rows.Add(row);
				counter++;
			}

			return new ProductCodesScanningReport(createDateFrom, createDateTo, rows);
		}

		public class Row
		{
			public int RowNumber { get; set; }
			public string DriverFIO { get; set; }
			public int TotalCodesCount { get; set; }
			public int SuccessfullyScannedCodesCount { get; set; }
			public decimal SuccessfullyScannedCodesPercent => (SuccessfullyScannedCodesCount / TotalCodesCount) * 100;
			public int UnscannedCodesCount { get; set; }
			public decimal UnscannedCodesPercent { get; set; }
			public int SingleDuplicatedCodesCount { get; set; }
			public decimal SingleDuplicatedCodesPercent { get; set; }
			public int MultiplyDuplicatedCodesCount { get; set; }
			public decimal MultiplyDuplicatedCodesPercent { get; set; }
			public int InvalidCodesCount { get; set; }
			public decimal InvalidCodesPercent { get; set; }
		}
	}
}
