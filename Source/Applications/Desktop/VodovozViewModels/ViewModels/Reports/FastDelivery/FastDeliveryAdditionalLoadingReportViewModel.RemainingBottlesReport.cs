using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.ViewModels.Reports.FastDelivery
{
	public partial class FastDeliveryAdditionalLoadingReportViewModel
	{
		[Appellative(Nominative = "Отчёт по остатку бутылей")]
		public partial class RemainingBottlesReport
		{
			private RemainingBottlesReport(DateTime createDateFrom, DateTime createDateTo, List<Row> rows)
			{
				CreateDateFrom = createDateFrom;
				CreateDateTo = createDateTo;
				Rows = rows;
				ReportCreatedAt = DateTime.Now;
			}

			public string Title => typeof(RemainingBottlesReport).GetClassUserFriendlyName().Nominative;

			public DateTime CreateDateFrom { get; }

			public DateTime CreateDateTo { get; }

			public DateTime ReportCreatedAt { get; }

			public List<Row> Rows { get; set; }

			public static RemainingBottlesReport Generate(IUnitOfWork unitOfWork, DateTime createDateFrom, DateTime createDateTo)
			{
				var notActualRouteListStatuses = RouteListItem.GetNotDeliveredStatuses();

				var rows = (from routelist in unitOfWork.Session.Query<RouteList>()
							join driver in unitOfWork.Session.Query<Employee>()
							on routelist.Driver.Id equals driver.Id
							join additionalLoadingDocument in unitOfWork.Session.Query<AdditionalLoadingDocument>()
							on routelist.AdditionalLoadingDocument.Id equals additionalLoadingDocument.Id
							where routelist.Date >= createDateFrom
								&& routelist.Date <= createDateTo
							let shift = (from deliveryShift in unitOfWork.Session.Query<DeliveryShift>()
										 where deliveryShift.Id == routelist.Shift.Id
										 select deliveryShift.Name).FirstOrDefault() ?? ""
							let bottlesLoadedAdditionallyCount = ((decimal?)(from additionalLoadingDocumentItem in unitOfWork.Session.Query<AdditionalLoadingDocumentItem>()
																			 join nomenclature in unitOfWork.Session.Query<Nomenclature>()
																			 on additionalLoadingDocumentItem.Nomenclature.Id equals nomenclature.Id
																			 where nomenclature.Category == NomenclatureCategory.water
																			   && nomenclature.TareVolume == TareVolume.Vol19L
																			   && additionalLoadingDocumentItem.AdditionalLoadingDocument.Id == additionalLoadingDocument.Id
																			 select additionalLoadingDocumentItem.Amount).Sum()) ?? 0
							let bottlesLoadedFromWarehouseTotalCount = ((decimal?)(from carLoadDocument in unitOfWork.Session.Query<CarLoadDocument>()
																				   join carLoadDocumentItem in unitOfWork.Session.Query<CarLoadDocumentItem>()
																				   on carLoadDocument.Id equals carLoadDocumentItem.Document.Id
																				   join nomenclature in unitOfWork.Session.Query<Nomenclature>()
																				   on carLoadDocumentItem.Nomenclature.Id equals nomenclature.Id
																				   where nomenclature.Category == NomenclatureCategory.water
																				    && nomenclature.TareVolume == TareVolume.Vol19L
																					&& routelist.Id == carLoadDocument.RouteList.Id
																				   select carLoadDocumentItem.Amount).Sum()) ?? 0
							let bottlesLoadedFromDriversCount = ((decimal?)(from addressTransferDocument in unitOfWork.Session.Query<AddressTransferDocument>()
																			join addressTransferDocumentItem in unitOfWork.Session.Query<AddressTransferDocumentItem>()
																			on addressTransferDocument.Id equals addressTransferDocumentItem.Document.Id
																			join driverNomenclatureTransferItem in unitOfWork.Session.Query<DriverNomenclatureTransferItem>()
																			on addressTransferDocumentItem.Id equals driverNomenclatureTransferItem.DocumentItem.Id
																			join nomenclature in unitOfWork.Session.Query<Nomenclature>()
																			on driverNomenclatureTransferItem.Nomenclature.Id equals nomenclature.Id
																			where nomenclature.Category == NomenclatureCategory.water
																			 && nomenclature.TareVolume == TareVolume.Vol19L
																			 && addressTransferDocument.RouteListTo.Id == routelist.Id
																			 && addressTransferDocumentItem.AddressTransferType == AddressTransferType.FromHandToHand
																			select driverNomenclatureTransferItem.Amount).Sum()) ?? 0m
							let bottlesShippedFastDeliveryCount = ((decimal?)(from routeListAddress in unitOfWork.Session.Query<RouteListItem>()
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
																			   && order.CreateDate >= createDateFrom
																			   && order.CreateDate <= createDateTo
																			  select orderItem.ActualCount ?? orderItem.Count).Sum()) ?? 0m
							let bottlesShippedPlanCount = ((decimal?)(from routeListAddress in unitOfWork.Session.Query<RouteListItem>()
																	  join order in unitOfWork.Session.Query<Order>()
																	  on routeListAddress.Order.Id equals order.Id
																	  join orderItem in unitOfWork.Session.Query<OrderItem>()
																	  on order.Id equals orderItem.Order.Id
																	  join nomenclature in unitOfWork.Session.Query<Nomenclature>()
																	  on orderItem.Nomenclature.Id equals nomenclature.Id
																	  where !order.IsFastDelivery
																	   && routeListAddress.Status == RouteListItemStatus.Completed
																	   && routeListAddress.RouteList.Id == routelist.Id
																	   && nomenclature.Category == NomenclatureCategory.water
																	   && nomenclature.TareVolume == TareVolume.Vol19L
																	  select orderItem.ActualCount ?? orderItem.Count).Sum()) ?? 0m
							let bottlesTransferedToDriversCount = ((decimal?)(from addressTransferDocument in unitOfWork.Session.Query<AddressTransferDocument>()
																			  join addressTransferDocumentItem in unitOfWork.Session.Query<AddressTransferDocumentItem>()
																			  on addressTransferDocument.Id equals addressTransferDocumentItem.Document.Id
																			  join driverNomenclatureTransferItem in unitOfWork.Session.Query<DriverNomenclatureTransferItem>()
																			  on addressTransferDocumentItem.Id equals driverNomenclatureTransferItem.DocumentItem.Id
																			  join nomenclature in unitOfWork.Session.Query<Nomenclature>()
																			  on driverNomenclatureTransferItem.Nomenclature.Id equals nomenclature.Id
																			  where nomenclature.Category == NomenclatureCategory.water
																			   && nomenclature.TareVolume == TareVolume.Vol19L
																			   && addressTransferDocument.RouteListFrom.Id == routelist.Id
																			   && addressTransferDocumentItem.AddressTransferType == AddressTransferType.FromHandToHand
																			  select driverNomenclatureTransferItem.Amount).Sum()) ?? 0m
							let addressesCount = (from order in unitOfWork.Session.Query<Order>()
												  join routeListAddress in unitOfWork.Session.Query<RouteListItem>()
												  on order.Id equals routeListAddress.Order.Id
												  where routeListAddress.RouteList.Id == routelist.Id
													&& !notActualRouteListStatuses.Contains(routeListAddress.Status)
												  select order.Id).Count()
							let driverfullNameWithInitials = $"{driver.LastName} " +
								$"{driver.Name.Substring(0, 1)}" +
								$".{driver.Patronymic.Substring(0, 1)}."
							select new Row
							{
								Shift = shift,
								CreationDate = routelist.Date,
								RouteListId = routelist.Id,
								DriverFullName = driverfullNameWithInitials,
								BottlesLoadedAdditionallyCount = bottlesLoadedAdditionallyCount,
								BottlesLoadedPlanCount = bottlesLoadedFromWarehouseTotalCount - bottlesLoadedAdditionallyCount,
								BottlesLoadedFromOtherDriversCount = bottlesLoadedFromDriversCount,
								BottlesShippedFastDeliveryCount = bottlesShippedFastDeliveryCount,
								BottlesShippedPlanCount = bottlesShippedPlanCount,
								BottlesTransferedToOtherDriversCount = bottlesTransferedToDriversCount,
								RemainingBottlesCount = bottlesLoadedFromWarehouseTotalCount + bottlesLoadedFromDriversCount - bottlesShippedFastDeliveryCount - bottlesShippedPlanCount - bottlesTransferedToDriversCount,
								AddressesCount = addressesCount
							}).ToList();

				return new RemainingBottlesReport(createDateFrom, createDateTo, rows);
			}
		}
	}
}
