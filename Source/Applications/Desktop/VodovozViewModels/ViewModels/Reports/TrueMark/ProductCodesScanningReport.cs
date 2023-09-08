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
			CashReceipt cashReceiptAlias = null;
			CashReceiptProductCode cashReceiptProductCodeAlias = null;
			RouteListItem routeListItemAlias = null;
			RouteList routeListAlias = null;
			Employee driverAlias = null;
			TrueMarkWaterIdentificationCode trueMarkWaterIdentificationCodeAlias = null;
			ScannedCodeInfo resultAlias = null;

			var driverFioProjection = CustomProjections.Concat_WS(" ",
				Projections.Property(() => driverAlias.LastName),
				Projections.Property(() => driverAlias.Name),
				Projections.Property(() => driverAlias.Patronymic)
			);

			var isProductCodeSingleDuplicatedProjection = Projections.Conditional(
				Restrictions.Conjunction()
					.Add(() => cashReceiptProductCodeAlias.IsDuplicateSourceCode)
					.Add(() => cashReceiptProductCodeAlias.DuplicatsCount <= 1),
				Projections.Constant(true),
				Projections.Constant(false)
				);

			var isProductCodeMultiplyDuplicatedProjection = Projections.Conditional(
				Restrictions.Conjunction()
					.Add(() => cashReceiptProductCodeAlias.IsDuplicateSourceCode)
					.Add(() => cashReceiptProductCodeAlias.DuplicatsCount > 1),
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
				.Where(() => cashReceiptAlias.CreateDate >= createDateFrom
					&& cashReceiptAlias.CreateDate < createDateTo
					&& !cashReceiptAlias.WithoutMarks)
				.SelectList(list => list
					.Select(() => driverAlias.Id).WithAlias(() => resultAlias.DriverId)
					.Select(driverFioProjection).WithAlias(() => resultAlias.DriverFIO)
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
				.GroupBy(c => new { c.DriverId, c.DriverFIO })
				.ToDictionary(g => g.Key, g => g.ToList());

			var rows = new List<Row>();
			var counter = 1;

			foreach(var item in groupedByDriverCodes)
			{
				var driver = item.Key.DriverFIO;
				var codes = item.Value;

				var row = new Row();

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
