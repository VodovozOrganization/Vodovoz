using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public partial class FastDeliveryAdditionalLoadingReportViewModel
	{
		[Appellative(Nominative = "Отчёт по продажам с доставкой за час")]
		public partial class FastDeliveryRemainingBottlesReport
		{
			private FastDeliveryRemainingBottlesReport(DateTime createDateFrom, DateTime createDateTo, List<Row> rows)
			{
				CreateDateFrom = createDateFrom;
				CreateDateTo = createDateTo;
				Rows = rows;
				ReportCreatedAt = DateTime.Now;
			}

			public string Title => typeof(FastDeliveryRemainingBottlesReport).GetClassUserFriendlyName().Nominative;

			public DateTime CreateDateFrom { get; }

			public DateTime CreateDateTo { get; }

			public DateTime ReportCreatedAt { get; }

			public List<Row> Rows { get; set; }

			public static FastDeliveryRemainingBottlesReport Generate(IUnitOfWork unitOfWork, DateTime createDateFrom, DateTime createDateTo)
			{
				var notActualRouteListStatuses = new RouteListItemStatus[] {
					RouteListItemStatus.Canceled,
					RouteListItemStatus.Overdue,
					RouteListItemStatus.Transfered
				};

				var rows = (from routelist in unitOfWork.Session.Query<RouteList>()
							join driver in unitOfWork.Session.Query<Employee>()
							on routelist.Driver.Id equals driver.Id
							join additionalLoadingDocument in unitOfWork.Session.Query<AdditionalLoadingDocument>()
							on routelist.AdditionalLoadingDocument.Id equals additionalLoadingDocument.Id
							where routelist.Date >= createDateFrom
								&& routelist.Date <= createDateTo
							let bottlesLoadedCount = (from additionalLoadingDocumentItem in unitOfWork.Session.Query<AdditionalLoadingDocumentItem>()
													  join nomenclature in unitOfWork.Session.Query<Nomenclature>()
													  on additionalLoadingDocumentItem.Nomenclature.Id equals nomenclature.Id
													  where nomenclature.Category == NomenclatureCategory.water
														&& nomenclature.TareVolume == TareVolume.Vol19L
														&& additionalLoadingDocumentItem.AdditionalLoadingDocument.Id == additionalLoadingDocument.Id
													  select additionalLoadingDocumentItem.Amount).Sum()
							let bottlesShippedCount = ((decimal?)(from routeListAddress in unitOfWork.Session.Query<RouteListItem>()
													   join order in unitOfWork.Session.Query<Order>()
													   on routeListAddress.Order.Id equals order.Id
													   join orderItem in unitOfWork.Session.Query<OrderItem>()
													   on order.Id equals orderItem.Order.Id
													   join nomenclature in unitOfWork.Session.Query<Nomenclature>()
													   on orderItem.Nomenclature.Id equals nomenclature.Id
													   where order.IsFastDelivery
														&& routeListAddress.Status == RouteListItemStatus.Completed
														&& routeListAddress.RouteList.Id == routelist.Id
														&& nomenclature.Category == NomenclatureCategory.water
														&& nomenclature.TareVolume == TareVolume.Vol19L
													   select orderItem.ActualCount ?? orderItem.Count).Sum()) ?? 0m
							let addressesCount = (from order in unitOfWork.Session.Query<Order>()
												  join routeListAddress in unitOfWork.Session.Query<RouteListItem>()
												  on order.Id equals routeListAddress.Order.Id
												  where routeListAddress.RouteList.Id == routelist.Id
													&& order.IsFastDelivery
													&& !notActualRouteListStatuses.Contains(routeListAddress.Status)
												  select order.Id).Count()
							select new Row
							{
								CreationDate = routelist.Date,
								RouteListId = routelist.Id,
								DriverFullName = $"{driver.Name} {driver.LastName} {driver.Patronymic}",
								BottlesLoadedCount = bottlesLoadedCount,
								BottlesShippedCount = bottlesShippedCount,
								RemainingBottlesCount = bottlesLoadedCount - bottlesShippedCount,
								AddressesCount = addressesCount
							}).ToList();

				return new FastDeliveryRemainingBottlesReport(createDateFrom, createDateTo, rows);
			}
		}
	}
}
