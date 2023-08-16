using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public partial class FastDeliveryAdditionalLoadingReportViewModel
	{
		[Appellative(Nominative = "Отчёт по продажам с доставкой за час")]
		public class FastDeliveryRemainingBottlesReport
		{
			private FastDeliveryRemainingBottlesReport(DateTime createDateFrom, DateTime createDateTo, List<Row> rows)
			{
				CreateDateFrom = createDateFrom;
				CreateDateTo = createDateTo;
				Rows = rows;
				ReportCreatedAt = DateTime.Now;
			}

			public DateTime CreateDateFrom { get; }

			public DateTime CreateDateTo { get; }

			public DateTime ReportCreatedAt { get; }

			public List<Row> Rows { get; set; }

			public static FastDeliveryRemainingBottlesReport Generate(IUnitOfWork unitOfWork, DateTime createDateFrom, DateTime createDateTo)
			{
				var rows = (from routelist in unitOfWork.Session.Query<RouteList>()
							select new Row
							{
								RouteListId = routelist.Id
							}).ToList();

				return new FastDeliveryRemainingBottlesReport(createDateFrom, createDateTo, rows);
			}

			public class Row
			{
				public string Route { get; set; }

				public DateTime CreationDate { get; set; }

				public int RouteListId { get; set; }

				public string DriverFullName { get; set; }

				public int BottlesLoadedCount { get; set; }

				public int BottlesShippedCount { get; set; }

				public int RemainingBottlesCount { get; set; }

				public int AddressesCount { get; set; }
			}
		}
	}
}
