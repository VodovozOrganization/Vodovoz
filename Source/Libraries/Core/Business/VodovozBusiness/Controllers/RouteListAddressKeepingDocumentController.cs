using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Services;

namespace Vodovoz.Controllers
{
	public class RouteListAddressKeepingDocumentController : IRouteListAddressKeepingDocumentController
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

		private void CreateOperationsForReturns(IUnitOfWork uow, RouteListItem routeListItem, RouteListAddressKeepingDocument routeListKeepingDocument, RouteListItemStatus? oldStatus, RouteListItemStatus? newStatus)
		{
			var routeList = routeListItem.RouteList;

			var defaultBottleNomenclature = _nomenclatureParametersProvider.GetDefaultBottleNomenclature(uow);
			var pickupEquipments = routeListItem.Order.OrderEquipments.Where(x => x.Direction == Direction.PickUp).ToList();
			sbyte amountSign;
			if(newStatus != RouteListItemStatus.Completed && oldStatus == RouteListItemStatus.Completed)
			{
				amountSign = -1;
			}
			else if(newStatus == RouteListItemStatus.Completed && oldStatus != RouteListItemStatus.Completed)
			{
				amountSign = 1;
			}
			else
			{
				return;
			}

			var bottleRouteListKeepingDocumentItem = new RouteListAddressKeepingDocumentItem
			{
				RouteListAddressKeepingDocument = routeListKeepingDocument,
				Nomenclature = defaultBottleNomenclature,
				Amount = (routeListItem.DriverBottlesReturned ?? ((routeListItem.Order.BottlesReturn ?? 0) + routeListItem.Order.BottlesByStockCount)) * amountSign
			};

			bottleRouteListKeepingDocumentItem.CreateOrUpdateOperation();
			routeListKeepingDocument.Items.Add(bottleRouteListKeepingDocumentItem);

			routeList.ObservableDeliveryFreeBalanceOperations.Add(bottleRouteListKeepingDocumentItem.DeliveryFreeBalanceOperation);

			var isOldUndelivered = RouteListItem.GetUndeliveryStatuses().Contains(oldStatus.Value);

			foreach(var item in pickupEquipments)
			{
				var routeListKeepingDocumentItem = new RouteListAddressKeepingDocumentItem
				{
					RouteListAddressKeepingDocument = routeListKeepingDocument,
					Nomenclature = item.Nomenclature,
					Amount = (isOldUndelivered ? item.Count : item.CurrentCount) * amountSign
				};

				routeListKeepingDocumentItem.CreateOrUpdateOperation();
				routeListKeepingDocument.Items.Add(routeListKeepingDocumentItem);

				routeList.ObservableDeliveryFreeBalanceOperations.Add(routeListKeepingDocumentItem.DeliveryFreeBalanceOperation);
			}
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
			if(newStatus == RouteListItemStatus.Transfered || oldStatus == RouteListItemStatus.Transfered)
			{
				return;
			}

			var balanceType = GetDeliveryFreeBalanceType(oldStatus, newStatus);

			CreateOrUpdateRouteListKeepingDocument(uow, routeListItem, balanceType, false, false, oldStatus, newStatus);
		}

		public void CreateOrUpdateRouteListKeepingDocument(IUnitOfWork uow, RouteListItem routeListItem,
			DeliveryFreeBalanceType deliveryFreeBalanceType, bool isFullRecreation = false, bool isActualCount = false,
			RouteListItemStatus? oldStatus = null, RouteListItemStatus? newStatus = null)
		{
			var routeListKeepingDocument =
				uow.GetAll<RouteListAddressKeepingDocument>()
					.SingleOrDefault(x => x.RouteListItem.Id == routeListItem.Id)
				?? new RouteListAddressKeepingDocument();

			var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(uow);
			routeListKeepingDocument.RouteListItem = routeListItem;
			routeListKeepingDocument.Author = currentEmployee;

			var routeList = routeListItem.RouteList;

			if(deliveryFreeBalanceType == DeliveryFreeBalanceType.Unchange)
			{
				CreateOperationsForReturns(uow, routeListItem, routeListKeepingDocument, oldStatus, newStatus);
				uow.Save(routeListKeepingDocument);
				return;
			}

			sbyte amountSign;
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
					uow.Delete(item);
				}

				routeListKeepingDocument.Items.Clear();
			}

			CreateOperationsForReturns(uow, routeListItem, routeListKeepingDocument, oldStatus, newStatus);

			foreach(var item in routeListItem.Order.GetAllGoodsToDeliver(isActualCount))
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

		public void CreateOrUpdateRouteListKeepingDocument(IUnitOfWork uoW, RouteList routeList, DeliveryFreeBalanceType deliveryFreeBalanceType, bool isFullRecreation, bool isActualCount)
		{
			foreach(var address in routeList.Addresses)
			{
				CreateOrUpdateRouteListKeepingDocument(uoW, address, deliveryFreeBalanceType, isFullRecreation, isActualCount, newStatus: RouteListItemStatus.Completed);
			}
		}

		public IList<RouteListAddressKeepingDocumentItem> CreateOrUpdateRouteListKeepingDocumentByDiscrepancy(
			IUnitOfWork uow, RouteListItem changedRouteListItem, IList<RouteListAddressKeepingDocumentItem> itemsCacheList = null)
		{
			var routeListKeepingDocument =
				uow.GetAll<RouteListAddressKeepingDocument>()
					.SingleOrDefault(x => x.RouteListItem.Id == changedRouteListItem.Id)
				?? new RouteListAddressKeepingDocument();

			if(itemsCacheList != null)
			{
				foreach(var item in itemsCacheList)
				{
					changedRouteListItem.RouteList.ObservableDeliveryFreeBalanceOperations.Remove(item.DeliveryFreeBalanceOperation);
					routeListKeepingDocument.Items.Remove(item);
				}
			}

			IList<NomenclatureAmountNode> oldGoodsToDeliverAmountNodes;
			IList<NomenclatureAmountNode> oldEquipmentToPickupAmountNodes;
			RouteListItem oldRouteListItem;
			var newItems = new List<RouteListAddressKeepingDocumentItem>();

			using(var uowLocal = UnitOfWorkFactory.CreateWithoutRoot("Измениние свободных остатков на кассе"))
			{
				oldRouteListItem = uowLocal.GetById<RouteListItem>(changedRouteListItem.Id);
				oldGoodsToDeliverAmountNodes = oldRouteListItem.Order.GetAllGoodsToDeliver(true);
				oldEquipmentToPickupAmountNodes = oldRouteListItem.Order.OrderEquipments
					.Where(x => x.Direction == Direction.PickUp)
					.GroupBy(n => n.Nomenclature.Id)
					.Select(n => new NomenclatureAmountNode
					{
						NomenclatureId = n.Key,
						Nomenclature = n.First().Nomenclature,
						Amount = n.Sum(s => s.CurrentCount)
					})
					.ToList();
			}

			var currentEmployee = _employeeRepository.GetEmployeeForCurrentUser(uow);
			routeListKeepingDocument.RouteListItem = changedRouteListItem;
			routeListKeepingDocument.Author = currentEmployee;

			#region ToDeliver

			sbyte amountSign = -1;

			foreach(var node in oldGoodsToDeliverAmountNodes)
			{
				decimal count;

				var foundInChanged = changedRouteListItem.Order.OrderItems
					.Select(i => new NomenclatureAmountNode
					{
						NomenclatureId = i.Nomenclature.Id,
						Nomenclature = i.Nomenclature,
						Amount = i.CurrentCount
					})
					.Concat(changedRouteListItem.Order.OrderEquipments
						.Where(e => e.Direction == Direction.Deliver)
						.Select(e => new NomenclatureAmountNode
						{
							NomenclatureId = e.Nomenclature.Id,
							Nomenclature = e.Nomenclature,
							Amount = e.CurrentCount
						}))
					.Where(n => n.Nomenclature.Id == node.NomenclatureId)
					.GroupBy(n => n.Nomenclature.Id)
					.Select(n => new NomenclatureAmountNode
					{
						NomenclatureId = n.Key,
						Nomenclature = n.First().Nomenclature,
						Amount = n.Sum(s => s.Amount)
					})
					.SingleOrDefault();

				if(foundInChanged != null)
				{
					if(foundInChanged.Amount == node.Amount)
					{
						continue;
					}

					count = foundInChanged.Amount - node.Amount;
				}
				else
				{
					count = -node.Amount;
				}

				var routeListKeepingDocumentItem = new RouteListAddressKeepingDocumentItem();
				routeListKeepingDocumentItem.RouteListAddressKeepingDocument = routeListKeepingDocument;
				routeListKeepingDocumentItem.Nomenclature = node.Nomenclature;
				routeListKeepingDocumentItem.Amount = count * amountSign;
				routeListKeepingDocument.Items.Add(routeListKeepingDocumentItem);
				routeListKeepingDocumentItem.CreateOrUpdateOperation();
				changedRouteListItem.RouteList.ObservableDeliveryFreeBalanceOperations.Add(routeListKeepingDocumentItem.DeliveryFreeBalanceOperation);
				newItems.Add(routeListKeepingDocumentItem);
			}

			var newItemsToDeliver = changedRouteListItem.Order.GetAllGoodsToDeliver(true)
				.Where(x => oldGoodsToDeliverAmountNodes.All(a => a.Nomenclature.Id != x.Nomenclature.Id));

			foreach(var item in newItemsToDeliver)
			{
				var routeListKeepingDocumentItem = new RouteListAddressKeepingDocumentItem();
				routeListKeepingDocumentItem.RouteListAddressKeepingDocument = routeListKeepingDocument;
				routeListKeepingDocumentItem.Nomenclature = item.Nomenclature;
				routeListKeepingDocumentItem.Amount = item.Amount * amountSign;
				routeListKeepingDocument.Items.Add(routeListKeepingDocumentItem);
				routeListKeepingDocumentItem.CreateOrUpdateOperation();
				changedRouteListItem.RouteList.ObservableDeliveryFreeBalanceOperations.Add(routeListKeepingDocumentItem.DeliveryFreeBalanceOperation);
				newItems.Add(routeListKeepingDocumentItem);
			}

			#endregion

			#region Pickup

			foreach(var node in oldEquipmentToPickupAmountNodes)
			{
				decimal count;

				var foundInChanged = changedRouteListItem.Order.OrderEquipments
					.Where(x => x.Direction == Direction.PickUp)
					.GroupBy(n => n.Nomenclature.Id)
					.Select(n => new NomenclatureAmountNode
					{
						NomenclatureId = n.Key,
						Nomenclature = n.First().Nomenclature,
						Amount = n.Sum(s => s.CurrentCount)
					})
					.ToList()
					.SingleOrDefault(x => x.Nomenclature.Id == node.NomenclatureId);

				if(foundInChanged != null)
				{
					if(foundInChanged.Amount == node.Amount)
					{
						continue;
					}

					count = foundInChanged.Amount - node.Amount;
				}
				else
				{
					count = -node.Amount;
				}

				var routeListKeepingDocumentItem = new RouteListAddressKeepingDocumentItem();
				routeListKeepingDocumentItem.RouteListAddressKeepingDocument = routeListKeepingDocument;
				routeListKeepingDocumentItem.Nomenclature = node.Nomenclature;
				routeListKeepingDocumentItem.Amount = count;
				routeListKeepingDocument.Items.Add(routeListKeepingDocumentItem);
				routeListKeepingDocumentItem.CreateOrUpdateOperation();
				changedRouteListItem.RouteList.ObservableDeliveryFreeBalanceOperations.Add(routeListKeepingDocumentItem.DeliveryFreeBalanceOperation);
				newItems.Add(routeListKeepingDocumentItem);
			}

			var newEquipmentsToPickup = changedRouteListItem.Order.OrderEquipments
				.Where(x => x.Direction == Direction.PickUp)
				.Where(x => oldEquipmentToPickupAmountNodes
					.All(old => old.Nomenclature.Id != x.Nomenclature.Id))
				.ToList();

			foreach(var item in newEquipmentsToPickup)
			{
				var routeListKeepingDocumentItem = new RouteListAddressKeepingDocumentItem();
				routeListKeepingDocumentItem.RouteListAddressKeepingDocument = routeListKeepingDocument;
				routeListKeepingDocumentItem.Nomenclature = item.Nomenclature;
				routeListKeepingDocumentItem.Amount = item.CurrentCount;
				routeListKeepingDocument.Items.Add(routeListKeepingDocumentItem);
				routeListKeepingDocumentItem.CreateOrUpdateOperation();
				changedRouteListItem.RouteList.ObservableDeliveryFreeBalanceOperations.Add(routeListKeepingDocumentItem.DeliveryFreeBalanceOperation);
				newItems.Add(routeListKeepingDocumentItem);
			}

			#endregion

			#region bottles

			sbyte bottlesAmountSign = 0;
			if(changedRouteListItem.Status != RouteListItemStatus.Completed && oldRouteListItem.Status == RouteListItemStatus.Completed)
			{
				bottlesAmountSign = -1;
			}
			else if(changedRouteListItem.Status == RouteListItemStatus.Completed && oldRouteListItem.Status != RouteListItemStatus.Completed)
			{
				bottlesAmountSign = 1;
			}

			if(bottlesAmountSign != 0)
			{
				var defaultBottleNomenclature = _nomenclatureParametersProvider.GetDefaultBottleNomenclature(uow);

				var bottleRouteListKeepingDocumentItem = new RouteListAddressKeepingDocumentItem
				{
					RouteListAddressKeepingDocument = routeListKeepingDocument,
					Nomenclature = defaultBottleNomenclature,
					Amount = (changedRouteListItem.DriverBottlesReturned ?? changedRouteListItem.Order.BottlesReturn ?? 0) * bottlesAmountSign
				};

				bottleRouteListKeepingDocumentItem.CreateOrUpdateOperation();
				routeListKeepingDocument.Items.Add(bottleRouteListKeepingDocumentItem);
				newItems.Add(bottleRouteListKeepingDocumentItem);

				changedRouteListItem.RouteList.ObservableDeliveryFreeBalanceOperations.Add(bottleRouteListKeepingDocumentItem.DeliveryFreeBalanceOperation);
			}

			#endregion

			uow.Save(routeListKeepingDocument);

			return newItems;
		}

		public void RemoveRouteListKeepingDocument(IUnitOfWork uow, RouteListItem routeListItem)
		{
			var routeListKeepingDocument =
				uow.GetAll<RouteListAddressKeepingDocument>()
					.SingleOrDefault(x => x.RouteListItem.Id == routeListItem.Id);

			if(routeListKeepingDocument == null)
			{
				return;
			}

			foreach(var item in routeListKeepingDocument.Items)
			{
				routeListItem.RouteList.ObservableDeliveryFreeBalanceOperations.Remove(item.DeliveryFreeBalanceOperation);
				uow.Delete(item);
			}

			routeListKeepingDocument.Items.Clear();

			uow.Delete(routeListKeepingDocument);
		}
	}
}
