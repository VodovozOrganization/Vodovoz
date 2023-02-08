using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Controllers
{
	public class RouteListAddressKeepingDocumentController: IRouteListAddressKeepingDocumentController
	{
		private readonly IEmployeeRepository _employeeRepository;
		private readonly INomenclatureParametersProvider _nomenclatureParametersProvider = new NomenclatureParametersProvider(new ParametersProvider());

		public RouteListAddressKeepingDocumentController(IEmployeeRepository employeeRepository, INomenclatureParametersProvider nomenclatureParametersProvider)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_nomenclatureParametersProvider = nomenclatureParametersProvider ?? throw new ArgumentNullException(nameof(nomenclatureParametersProvider));
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
				uow.GetAll<RouteListAddressKeepingDocument>()
					.SingleOrDefault(x => x.RouteListItem.Id == routeListItem.Id)
				?? new RouteListAddressKeepingDocument();

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

			foreach(var item in routeListKeepingDocument.Items)
			{
				uow.Delete(item);
			}

			routeListKeepingDocument.Items.Clear();

			int amountSign = deliveryFreeBalanceType == DeliveryFreeBalanceType.Increase ? 1 : -1;

			foreach(var item in routeListItem.Order.GetAllGoodsToDeliver())
			{
				var routeListKeepingDocumentItem = new RouteListAddressKeepingDocumentItem();

				routeListKeepingDocumentItem.RouteListAddressKeepingDocument = routeListKeepingDocument;
				routeListKeepingDocumentItem.Nomenclature = item.Nomenclature;
				routeListKeepingDocumentItem.Amount = item.Amount * amountSign;

				routeListKeepingDocument.Items.Add(routeListKeepingDocumentItem);
				
				routeListKeepingDocumentItem.CreateOrUpdateOperation();

				routeList.ObservableDeliveryFreeBalanceOperations.Add(routeListKeepingDocumentItem.DeliveryFreeBalanceOperation);
			}

			// оборудование от клиента
			foreach(var item in routeListItem.Order.OrderEquipments.Where(x => x.Direction == Direction.PickUp))
			{
				var routeListKeepingDocumentItem = new RouteListAddressKeepingDocumentItem
				{
					RouteListAddressKeepingDocument = routeListKeepingDocument,
					Nomenclature = item.Nomenclature,
					Amount = item.Count * (-amountSign)
				};

				routeListKeepingDocumentItem.CreateOrUpdateOperation();
				routeListKeepingDocument.Items.Add(routeListKeepingDocumentItem);

				routeList.ObservableDeliveryFreeBalanceOperations.Add(routeListKeepingDocumentItem.DeliveryFreeBalanceOperation);
			}

			// бутыли на возврат
			var bottleRouteListKeepingDocumentItem = new RouteListAddressKeepingDocumentItem
			{
				RouteListAddressKeepingDocument = routeListKeepingDocument,
				Nomenclature = _nomenclatureParametersProvider.GetDefaultBottleNomenclature(uow),
				Amount = routeListItem.Order.BottlesReturn ?? 0 * (-amountSign)
			};

			bottleRouteListKeepingDocumentItem.CreateOrUpdateOperation();
			routeListKeepingDocument.Items.Add(bottleRouteListKeepingDocumentItem);

			routeList.ObservableDeliveryFreeBalanceOperations.Add(bottleRouteListKeepingDocumentItem.DeliveryFreeBalanceOperation);

			uow.Save(routeListKeepingDocument);
		}
	}
}
