using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Controllers
{
	public class RouteListFreeBalanceDocumentController: IRouteListFreeBalanceDocumentController
	{
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IRouteListRepository _routeListRepository;

		public RouteListFreeBalanceDocumentController(IEmployeeRepository employeeRepository, IRouteListRepository routeListRepository)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
		}

		private DeliveryFreeBalanceType GetDeliveryFreeBalanceType(RouteListItemStatus oldStatus, RouteListItemStatus newStatus)
		{
			if((IsNegativeStatus(oldStatus) || IsNeutralStatus(oldStatus))
			   && IsPositiveStatus(newStatus))
			{
				return DeliveryFreeBalanceType.Increase;
			}

			if(IsPositiveStatus(oldStatus) 
			   && (IsNegativeStatus(newStatus) || IsNeutralStatus(newStatus)))
			{
				return DeliveryFreeBalanceType.Decrease;
			}

			if((IsPositiveStatus(oldStatus) && IsPositiveStatus(newStatus))
			   || (IsPositiveStatus(oldStatus) && IsNeutralStatus(newStatus))
			   || (IsNegativeStatus(oldStatus) && IsNegativeStatus(newStatus))
			   || (IsNegativeStatus(oldStatus) && IsNeutralStatus(newStatus))
			   || (IsNeutralStatus(oldStatus) && IsNeutralStatus(newStatus))
			   || (IsNeutralStatus(oldStatus) && IsNegativeStatus(newStatus)))
			{
				return DeliveryFreeBalanceType.Unchange;
			}

			throw new ArgumentOutOfRangeException(nameof(oldStatus));
		}

		private bool IsPositiveStatus(RouteListItemStatus status) => new[] { RouteListItemStatus.Canceled, RouteListItemStatus.Overdue }.Contains(status);
		private bool IsNegativeStatus(RouteListItemStatus status) => new[] { RouteListItemStatus.EnRoute }.Contains(status);
		private bool IsNeutralStatus(RouteListItemStatus status) => new[] { RouteListItemStatus.Completed }.Contains(status);

		public void CreateOrUpdateRouteListKeepingDocument(IUnitOfWork uow, Order order, DeliveryFreeBalanceType deliveryFreeBalanceType)
		{
			var address = uow.GetAll<RouteListItem>().FirstOrDefault(x => x.Order.Id == order.Id);

			CreateOrUpdateRouteListKeepingDocument(uow, address, deliveryFreeBalanceType);
		}

		public void CreateOrUpdateRouteListKeepingDocument(IUnitOfWork uow, RouteListItem routeListItem, RouteListItemStatus oldStatus, RouteListItemStatus newStatus)
		{
			var balanceType = GetDeliveryFreeBalanceType(oldStatus, newStatus);

			CreateOrUpdateRouteListKeepingDocument(uow, routeListItem, balanceType);
		}

		public void CreateOrUpdateRouteListKeepingDocument(IUnitOfWork uow, RouteListItem routeListItem, DeliveryFreeBalanceType deliveryFreeBalanceType)
		{
			if(deliveryFreeBalanceType == DeliveryFreeBalanceType.Unchange
			   || routeListItem == null)
			{
				return;
			}

			var routeListKeepingDocument =
				uow.GetAll<RouteListKeepintDocument>()
					.SingleOrDefault(x => x.RouteListItem.Id == routeListItem.Id)
				?? new RouteListKeepintDocument();

			var oldSignIsDecrease = routeListKeepingDocument.Items.Any(x => x.Amount < 0);
			var oldSignIsIncrease = routeListKeepingDocument.Items.Any(x => x.Amount > 0);

			if(deliveryFreeBalanceType == DeliveryFreeBalanceType.Erase
			   || (oldSignIsDecrease && deliveryFreeBalanceType == DeliveryFreeBalanceType.Increase)
			   || (oldSignIsIncrease && deliveryFreeBalanceType == DeliveryFreeBalanceType.Decrease))
			{
				uow.Delete(routeListKeepingDocument);
				return;
			}

			var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(uow);

			routeListKeepingDocument.RouteListItem = routeListItem;
			routeListKeepingDocument.Author = currentEmployee;

			routeListKeepingDocument.Items.Clear();

			int amountSign = deliveryFreeBalanceType == DeliveryFreeBalanceType.Increase ? 1 : -1;

			foreach(var item in routeListItem.Order.GetAllGoodsToDeliver())
			{
				var routeListKeepingDocumentItem =
					routeListKeepingDocument.Items.SingleOrDefault(x => x.Nomenclature.Id == item.Nomenclature.Id)
					?? new RouteListKeepingDocumentItem();

				routeListKeepingDocumentItem.RouteListKeepintDocument = routeListKeepingDocument;
				routeListKeepingDocumentItem.Nomenclature = item.Nomenclature;
				routeListKeepingDocumentItem.Amount = item.Amount * amountSign;
				routeListKeepingDocumentItem.CreateOrUpdateOperation();

				if(!routeListKeepingDocument.Items.Contains(routeListKeepingDocumentItem))
				{
					routeListKeepingDocument.Items.Add(routeListKeepingDocumentItem);
				}
			}

			uow.Save(routeListKeepingDocument);
		}

		public void CreateOrUpdateCarUnderloadDocument(IUnitOfWork uow, RouteList routeList)
		{
			var carLoadDocuments = uow.GetAll<CarLoadDocument>()
				.Where(d => d.RouteList.Id == routeList.Id)
				.ToList();

			var carLoadDocumentItems = new List<CarLoadDocumentItem>();

			foreach(var carLoadDocument in carLoadDocuments)
			{
				carLoadDocument.UpdateAlreadyLoaded(uow, _routeListRepository);
				carLoadDocument.UpdateInRouteListAmount(uow, _routeListRepository);
				carLoadDocumentItems.AddRange(carLoadDocument.Items);
			}

			var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(uow);

			var carUnderloadDocument =
				uow.GetAll<CarUnderloadDocument>()
					.SingleOrDefault(x => x.RouteList.Id == routeList.Id)
				?? new CarUnderloadDocument();

			carUnderloadDocument.RouteList = routeList;
			carUnderloadDocument.Author = currentEmployee;

			carUnderloadDocument.Items.Clear();

			CreateUnderloadDocumentItems(carLoadDocumentItems, carUnderloadDocument);

			uow.Save(carUnderloadDocument);
		}

		private void CreateUnderloadDocumentItems(List<CarLoadDocumentItem> carLoadDocumentItems, CarUnderloadDocument carUnderloadDocument)
		{
			var groupedItems = carLoadDocumentItems.GroupBy(i => i.Nomenclature)
				.Select(g => new
				{
					Nomenclature = g.Key,
					Amount = g.Sum(s => s.Amount),
					AmountLoaded = g.Sum(s => s.AmountLoaded),
					AmountInRouteList = g.Sum(s => s.AmountInRouteList)
				});
			
			foreach(var loadItem in groupedItems)
			{
				var amount = loadItem.AmountInRouteList - loadItem.AmountLoaded - loadItem.Amount;

				if(amount == 0)
				{
					continue;
				}

				var underloadItem = carUnderloadDocument.Items.SingleOrDefault(x => x.Nomenclature.Id == loadItem.Nomenclature.Id)
				                    ?? new CarUnderloadDocumentItem();

				underloadItem.Amount = amount * -1;
				underloadItem.Nomenclature = loadItem.Nomenclature;
				underloadItem.CarUnderloadDocument = carUnderloadDocument;

				underloadItem.CreateOrUpdateOperation();

				carUnderloadDocument.Items.Add(underloadItem);
			}
		}
	}
}
