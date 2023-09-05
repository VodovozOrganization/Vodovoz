using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
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
			CashReceipt cashReceiptAlias = null;
			RouteList routeListAlias = null;
			RouteListItem routeListItemAlias = null;
			Employee driverAlias = null;
			CashReceiptProductCode productCodeAlias = null;
			//TrueMarkWaterIdentificationCode sourceCodeAlias = null;
			Row resultAlias = null;

			var successfullyScannedCodesSubquery = QueryOver.Of(() => productCodeAlias)
				.Where(() => !productCodeAlias.IsDuplicateSourceCode
					&& !productCodeAlias.IsUnscannedSourceCode
					&& productCodeAlias.SourceCode != null);

			var query = unitOfWork.Session.QueryOver(() => productCodeAlias)
				.JoinAlias(() => productCodeAlias.CashReceipt, () => cashReceiptAlias)
				.JoinEntityQueryOver(() => routeListItemAlias, Restrictions.Where(() => cashReceiptAlias.Order.Id == routeListItemAlias.Order.Id), JoinType.LeftOuterJoin)
				.Left.JoinAlias(() => routeListItemAlias.RouteList, () => routeListAlias)
				.Left.JoinAlias(() => routeListAlias.Driver, () => driverAlias)
				.Where(() => !cashReceiptAlias.WithoutMarks)
				.Where(
					Restrictions.Ge(
						Projections.SqlFunction("DATE",
							NHibernateUtil.Date,
							Projections.Property(() => cashReceiptAlias.CreateDate)),
						createDateFrom
					)
				)
				.Where(
					Restrictions.Le(
						Projections.SqlFunction("DATE",
							NHibernateUtil.Date,
							Projections.Property(() => cashReceiptAlias.CreateDate)),
						createDateTo
					)
				)
				.Select()
				//.SelectList(list => list
					//.SelectGroup(() => driverAlias.LastName).WithAlias(() => resultAlias.DriverFIO)
					//.SelectCount(() => productCodeAlias.Id).WithAlias(() => resultAlias.TotalCodesCount)
					//.SelectSubQuery(successfullyScannedCodesSubquery).WithAlias(() => resultAlias.SuccessfullyScannedCodesCount)
					)
				.List<object[]>()
				.ToDictionary<>
				//.TransformUsing(Transformers.AliasToBean<Row>());

			var rows = query.List<Row>();

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
