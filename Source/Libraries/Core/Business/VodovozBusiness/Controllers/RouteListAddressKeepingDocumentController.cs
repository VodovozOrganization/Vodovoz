using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.Services;

namespace Vodovoz.Controllers
{
	public class RouteListAddressKeepingDocumentController: IRouteListAddressKeepingDocumentController
	{
		private readonly IEmployeeRepository _employeeRepository;
		private readonly INomenclatureParametersProvider _nomenclatureParametersProvider;

		public RouteListAddressKeepingDocumentController(IEmployeeRepository employeeRepository, INomenclatureParametersProvider nomenclatureParametersProvider)
		{
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_nomenclatureParametersProvider = nomenclatureParametersProvider ?? throw new ArgumentNullException(nameof(nomenclatureParametersProvider));
		}

		private DeliveryFreeBalanceType GetDeliveryFreeBalanceType(RouteListItemStatus oldStatus, RouteListItemStatus newStatus)
		{
			if((IsNegative(oldStatus) || IsNeutral(oldStatus))
			   && IsPositive(newStatus))
			{
				return DeliveryFreeBalanceType.Increase;
			}

			if(IsPositive(oldStatus) 
			   && (IsNegative(newStatus) || IsNeutral(newStatus)))
			{
				return DeliveryFreeBalanceType.Decrease;
			}

			if((IsPositive(oldStatus) && IsPositive(newStatus))
			   || (IsPositive(oldStatus) && IsNeutral(newStatus))
			   || (IsNegative(oldStatus) && IsNegative(newStatus))
			   || (IsNegative(oldStatus) && IsNeutral(newStatus))
			   || (IsNeutral(oldStatus) && IsNeutral(newStatus))
			   || (IsNeutral(oldStatus) && IsNegative(newStatus)))
			{
				return DeliveryFreeBalanceType.Unchange;
			}

			throw new ArgumentOutOfRangeException(nameof(oldStatus));
		}

		private bool IsPositive(RouteListItemStatus status) => new[] { RouteListItemStatus.Canceled, RouteListItemStatus.Overdue }.Contains(status);
		private bool IsNegative(RouteListItemStatus status) => new[] { RouteListItemStatus.EnRoute }.Contains(status);
		private bool IsNeutral(RouteListItemStatus status) => new[] { RouteListItemStatus.Completed }.Contains(status);

		public void CreateOrUpdateRouteListKeepingDocument(IUnitOfWork uow, Order order, DeliveryFreeBalanceType deliveryFreeBalanceType)
		{
			var address = uow.GetAll<RouteListItem>().FirstOrDefault(x => x.Order.Id == order.Id);

			if(address != null)
			{
				CreateOrUpdateRouteListKeepingDocument(uow, address, deliveryFreeBalanceType);
			}
		}

		public void CreateOrUpdateRouteListKeepingDocument(IUnitOfWork uow, RouteListItem routeListItem, RouteListItemStatus oldStatus, RouteListItemStatus newStatus)
		{
			var balanceType = GetDeliveryFreeBalanceType(oldStatus, newStatus);

			CreateOrUpdateRouteListKeepingDocument(uow, routeListItem, balanceType, false, oldStatus, newStatus);
		}

		public void CreateOrUpdateRouteListKeepingDocument(IUnitOfWork uow, RouteListItem routeListItem,
			DeliveryFreeBalanceType deliveryFreeBalanceType, bool isFullRecreation = false, RouteListItemStatus? oldStatus = null, RouteListItemStatus? newStatus = null)
		{
			var routeListKeepingDocument =
				uow.GetAll<RouteListAddressKeepingDocument>()
					.SingleOrDefault(x => x.RouteListItem.Id == routeListItem.Id)
				?? new RouteListAddressKeepingDocument();

			var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(uow);
			routeListKeepingDocument.RouteListItem = routeListItem;
			routeListKeepingDocument.Author = currentEmployee;

			var routeList = routeListItem.RouteList;

			var defaultBottleNomenclature = _nomenclatureParametersProvider.GetDefaultBottleNomenclature(uow);
			var pickupEquipments = routeListItem.Order.OrderEquipments.Where(x => x.Direction == Direction.PickUp).ToList();

			#region Бутыли на возврат и оборудование от клиента

			if(newStatus == RouteListItemStatus.Completed && oldStatus != RouteListItemStatus.Completed)
			{
				var bottleRouteListKeepingDocumentItem = new RouteListAddressKeepingDocumentItem
				{
					RouteListAddressKeepingDocument = routeListKeepingDocument,
					Nomenclature = defaultBottleNomenclature,
					Amount = routeListItem.DriverBottlesReturned ?? routeListItem.Order.BottlesReturn ?? 0
				};

				bottleRouteListKeepingDocumentItem.CreateOrUpdateOperation();
				routeListKeepingDocument.Items.Add(bottleRouteListKeepingDocumentItem);

				routeList.ObservableDeliveryFreeBalanceOperations.Add(bottleRouteListKeepingDocumentItem.DeliveryFreeBalanceOperation);

				foreach(var item in pickupEquipments)
				{
					var routeListKeepingDocumentItem = new RouteListAddressKeepingDocumentItem
					{
						RouteListAddressKeepingDocument = routeListKeepingDocument,
						Nomenclature = item.Nomenclature,
						Amount = item.CurrentCount
					};

					routeListKeepingDocumentItem.CreateOrUpdateOperation();
					routeListKeepingDocument.Items.Add(routeListKeepingDocumentItem);

					routeList.ObservableDeliveryFreeBalanceOperations.Add(routeListKeepingDocumentItem.DeliveryFreeBalanceOperation);
				}
			}

			if(newStatus != RouteListItemStatus.Completed && oldStatus == RouteListItemStatus.Completed)
			{
				var defaultBottleNomenclatureItems = routeListKeepingDocument.Items
					.Where(x => x.Nomenclature.Id == defaultBottleNomenclature.Id)
					.ToList();

				foreach(var defaultBottleNomenclatureItem in defaultBottleNomenclatureItems)
				{
					routeListKeepingDocument.Items.Remove(defaultBottleNomenclatureItem);
					routeList.ObservableDeliveryFreeBalanceOperations.Remove(defaultBottleNomenclatureItem.DeliveryFreeBalanceOperation);
					uow.Delete(defaultBottleNomenclatureItem);
				}

				var pickupsEquipmentItems = routeListKeepingDocument.Items
					.Where(kdi => pickupEquipments.Any(pe => pe.Nomenclature.Id == kdi.Nomenclature.Id))
					.ToList();

				foreach(var pickupsEquipmentItem in pickupsEquipmentItems)
				{
					routeListKeepingDocument.Items.Remove(pickupsEquipmentItem);
					routeList.ObservableDeliveryFreeBalanceOperations.Remove(pickupsEquipmentItem.DeliveryFreeBalanceOperation);
					uow.Delete(pickupsEquipmentItem);
				}
			}

			#endregion

			if(deliveryFreeBalanceType == DeliveryFreeBalanceType.Unchange)
			{
				uow.Save(routeListKeepingDocument);
				return;
			}

			int amountSign;
			switch(deliveryFreeBalanceType)
			{
				case DeliveryFreeBalanceType.Increase:
					amountSign = 1;
					break;
				case DeliveryFreeBalanceType.Decrease:
					amountSign = -1;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(deliveryFreeBalanceType));
			}

			if(isFullRecreation)
			{
				foreach(var item in routeListKeepingDocument.Items)
				{
					routeList.ObservableDeliveryFreeBalanceOperations.Remove(item.DeliveryFreeBalanceOperation);
				}

				foreach(var item in routeListKeepingDocument.Items)
				{
					uow.Delete(item);
				}

				routeListKeepingDocument.Items.Clear();
			}

			foreach(var item in routeListItem.Order.GetAllGoodsToDeliver(false))
			{
				var routeListKeepingDocumentItem = new RouteListAddressKeepingDocumentItem();

				routeListKeepingDocumentItem.RouteListAddressKeepingDocument = routeListKeepingDocument;
				routeListKeepingDocumentItem.Nomenclature = item.Nomenclature;
				routeListKeepingDocumentItem.Amount = item.Amount * amountSign;

				routeListKeepingDocument.Items.Add(routeListKeepingDocumentItem);
				
				routeListKeepingDocumentItem.CreateOrUpdateOperation();

				routeList.ObservableDeliveryFreeBalanceOperations.Add(routeListKeepingDocumentItem.DeliveryFreeBalanceOperation);
			}

			uow.Save(routeListKeepingDocument);
		}
	}
}
