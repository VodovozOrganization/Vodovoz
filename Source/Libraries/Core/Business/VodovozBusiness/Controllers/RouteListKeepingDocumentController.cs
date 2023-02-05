using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.Controllers
{
	public class RouteListKeepingDocumentController: IRouteListKeepingDocumentController
	{
		private readonly IEmployeeRepository _employeeRepository;

		public RouteListKeepingDocumentController(IEmployeeRepository employeeRepository)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
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
			if(deliveryFreeBalanceType == DeliveryFreeBalanceType.Unchange || routeListItem == null)
			{
				return;
			}

			var routeListKeepingDocument =
				uow.GetAll<RouteListKeepintDocument>()
					.SingleOrDefault(x => x.RouteListItem.Id == routeListItem.Id)
				?? new RouteListKeepintDocument();

			var routeList = routeListItem.RouteList;

			foreach(var item in routeListKeepingDocument.Items)
			{
				routeList.ObservableDeliveryFreeBalanceOperations.Remove(item.DeliveryFreeBalanceOperation);
			}

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

				routeListKeepingDocument.Items.Add(routeListKeepingDocumentItem);

				routeList.ObservableDeliveryFreeBalanceOperations.Add(routeListKeepingDocumentItem.DeliveryFreeBalanceOperation);
			}

			uow.Save(routeListKeepingDocument);
		}
	}
}
