using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Extensions;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OrderFromOnlineOrderCreator : IOrderFromOnlineOrderCreator
	{
		private readonly IOrderSettings _orderSettings;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IOrderContractUpdater _contractUpdater;

		public OrderFromOnlineOrderCreator(
			IOrderSettings orderSettings,
			INomenclatureRepository nomenclatureRepository,
			INomenclatureSettings nomenclatureSettings,
			IOrderContractUpdater contractUpdater)
		{
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_contractUpdater = contractUpdater ?? throw new ArgumentNullException(nameof(contractUpdater));
		}

		public Order CreateOrderFromOnlineOrder(IUnitOfWork uow, Employee orderCreator, OnlineOrder onlineOrder)
		{
			var order = new Order
			{
				UoW = uow
			};

			return FillOrderFromOnlineOrder(uow, order, onlineOrder, author: orderCreator);
		}

		public Order FillOrderFromOnlineOrder(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			Employee author = null,
			bool manualCreation = false)
		{
			var paymentFrom = onlineOrder.OnlinePaymentSource.HasValue
				? uow.GetById<PaymentFrom>(
					onlineOrder.OnlinePaymentSource.Value.ConvertToPaymentFromId(_orderSettings))
				: null;

			if(author != null)
			{
				order.Author = author;
			}
			
			order.UpdateClient(onlineOrder.Counterparty, _contractUpdater, out var updateClientMessage);
			order.UpdateDeliveryPoint(onlineOrder.DeliveryPoint, _contractUpdater);
			order.SelfDelivery = onlineOrder.IsSelfDelivery;
			order.UpdateDeliveryDate(onlineOrder.DeliveryDate, _contractUpdater, out var updateDeliveryDateMessage);
			order.DeliverySchedule = onlineOrder.DeliverySchedule;
			order.IsFastDelivery = onlineOrder.IsFastDelivery;
			order.UpdatePaymentType(onlineOrder.OnlineOrderPaymentType.ToOrderPaymentType(), _contractUpdater);
			order.BottlesReturn = onlineOrder.BottlesReturn;
			order.OnlineOrder = onlineOrder.OnlinePayment;
			order.UpdatePaymentByCardFrom(paymentFrom, _contractUpdater);
			order.Trifle = onlineOrder.Trifle;
			order.DontArriveBeforeInterval = onlineOrder.DontArriveBeforeInterval;

			if(!string.IsNullOrWhiteSpace(onlineOrder.OnlineOrderComment))
			{
				order.Comment = onlineOrder.OnlineOrderComment;
				order.HasCommentForDriver = true;
			}
			
			if(!order.SelfDelivery)
			{
				order.CallBeforeArrivalMinutes = onlineOrder.CallBeforeArrivalMinutes;
				order.IsDoNotMakeCallBeforeArrival = !onlineOrder.CallBeforeArrivalMinutes.HasValue;
			}
			else
			{
				order.SelfDeliveryGeoGroup = onlineOrder.SelfDeliveryGeoGroup;
			}
			
			//TODO: скорее всего этот метод здесь избыточен, т.к. при заполнении других полей договор обновится
			_contractUpdater.UpdateOrCreateContract(uow, order);

			if(order.Client is null)
			{
				return order;
			}
			
			if(order.Client.ReasonForLeaving == ReasonForLeaving.Unknown)
			{
				order.Client.ReasonForLeaving = ReasonForLeaving.ForOwnNeeds;
			}
			
			FillOrderGoodsFromOnlineOrder(uow, order, onlineOrder.OnlineOrderItems, onlineOrder.OnlineRentPackages, manualCreation);
			
			return order;
		}

		private void FillOrderGoodsFromPartOrder(
			IUnitOfWork uow,
			Order order,
			PartOrderWithGoods partOrder)
		{
			AddOrderItems(uow, order, partOrder.Goods);
			AddOrderEquipments(uow, order, partOrder.OrderEquipments);
		}

		private void FillOrderGoodsFromOnlineOrder(
			IUnitOfWork uow,
			Order order,
			IEnumerable<IProduct> onlineOrderItems,
			IEnumerable<OnlineFreeRentPackage> onlineRentPackages,
			bool manualCreation)
		{
			AddOrderItems(uow, order, onlineOrderItems, manualCreation);
			AddFreeRentPackages(uow, order, onlineRentPackages);
		}

		private void AddOrderItems(
			IUnitOfWork uow,
			Order order,
			IEnumerable<IProduct> onlineOrderItems,
			bool manualCreation = false)
		{
			AddNomenclatures(uow, order, onlineOrderItems, manualCreation);
		}

		private void AddNomenclatures(
			IUnitOfWork uow,
			Order order,
			IEnumerable<IProduct> onlineOrderItems,
			bool manualCreation = false)
		{
			var onlineOrderPromoSets = onlineOrderItems
				.Where(x => x.PromoSet != null)
				.ToLookup(x => x.PromoSet.Id);

			var otherItems =
				onlineOrderItems
					.Where(x => x.PromoSet is null);

			TryAddPromoSets(uow, order, onlineOrderPromoSets);

			if(manualCreation)
			{
				TryAddOtherItemsFromManualCreationOrder(uow, order, otherItems);
			}
			else
			{
				TryAddOtherItemsFromAutoCreationOrder(uow, order, otherItems);
			}
		}

		private void TryAddPromoSets(IUnitOfWork uow, Order order, ILookup<int, IProduct> onlineOrderPromoSets)
		{
			var addedPromoSetsForNewClients = new Dictionary<int, bool>();
			
			foreach(var onlineOrderItemGroup in onlineOrderPromoSets)
			{
				var promoSet = onlineOrderItemGroup.First().PromoSet;
				
				if(promoSet.PromotionalSetForNewClients && addedPromoSetsForNewClients.Any())
				{
					continue;
				}

				var promoSetItemsCount = promoSet.PromotionalSetItems.Count;
				var onlinePromoItemsCount = onlineOrderItemGroup.Count();
				var promoSetCount = onlinePromoItemsCount / promoSetItemsCount;

				if(promoSetCount == default)
				{
					promoSetCount = 1;
				}

				for(var i = 0; i < promoSetCount; i++)
				{
					foreach(var proSetItem in promoSet.PromotionalSetItems)
					{
						order.AddNomenclature(
							uow,
							_contractUpdater,
							proSetItem.Nomenclature,
							proSetItem.Count,
							proSetItem.IsDiscountInMoney ? proSetItem.DiscountMoney : proSetItem.Discount,
							proSetItem.IsDiscountInMoney,
							null,
							proSetItem.PromoSet);
					}
					
					order.ObservablePromotionalSets.Add(promoSet);
					
					if(promoSet.PromotionalSetForNewClients)
					{
						addedPromoSetsForNewClients.Add(promoSet.Id, true);
						break;
					}
				}
			}
		}
		
		private void TryAddOtherItemsFromManualCreationOrder(IUnitOfWork uow, Order order, IEnumerable<IProduct> otherItems)
		{
			foreach(var product in otherItems)
			{
				if(product.Nomenclature is null)
				{
					continue;
				}
				
				if(_nomenclatureSettings.PaidDeliveryNomenclatureId == product.Nomenclature.Id
					|| _nomenclatureSettings.FastDeliveryNomenclatureId == product.Nomenclature.Id)
				{
					continue;
				}

				if(product is OnlineOrderItem onlineOrderItem
					&& onlineOrderItem.OnlineOrderErrorState.HasValue
					&& onlineOrderItem.OnlineOrderErrorState == OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable)
				{
					order.AddNomenclature(uow, _contractUpdater, product.Nomenclature, product.Count);
				}
				else
				{
					if(product.DiscountReason is null)
					{
						order.AddNomenclature(uow, _contractUpdater, product.Nomenclature, product.Count);
					}
					else
					{
						order.AddNomenclature(
							uow,
							_contractUpdater,
							product.Nomenclature,
							product.Count,
							product.GetDiscount,
							product.IsDiscountInMoney,
							product.DiscountReason);
					}
				}
			}
		}
		
		private void TryAddOtherItemsFromAutoCreationOrder(IUnitOfWork uow, Order order, IEnumerable<IProduct> onlineOrderItems)
		{
			foreach(var onlineOrderItem in onlineOrderItems)
			{
				if(onlineOrderItem.Nomenclature is null)
				{
					continue;
				}
				
				order.AddNomenclature(
					uow,
					_contractUpdater,
					onlineOrderItem.Nomenclature,
					onlineOrderItem.Count,
					onlineOrderItem.GetDiscount,
					onlineOrderItem.IsDiscountInMoney,
					onlineOrderItem.DiscountReason);
			}
		}

		private void AddFreeRentPackages(IUnitOfWork uow, Order order, IEnumerable<OnlineFreeRentPackage> onlineRentPackages)
		{
			foreach(var onlineRentPackage in onlineRentPackages)
			{
				if(onlineRentPackage.FreeRentPackage is null)
				{
					continue;
				}
				
				var rentPackage = onlineRentPackage.FreeRentPackage;
				
				var existingItems = order.OrderEquipments
					.Where(x => x.OrderRentDepositItem != null || x.OrderRentServiceItem != null)
					.Select(x => x.Nomenclature.Id)
					.Distinct()
					.ToArray();

				var anyNomenclature = _nomenclatureRepository.GetAvailableNonSerialEquipmentForRent(
					order.UoW,
					rentPackage.EquipmentKind,
					existingItems);
				
				order.AddFreeRent(uow, _contractUpdater, rentPackage, anyNomenclature);
			}
		}
		
		private void AddOrderEquipments(IUnitOfWork uow, Order order, IEnumerable<OrderEquipment> partOrderEquipments)
		{
			if(!partOrderEquipments.Any())
			{
				return;
			}

			foreach(var equipment in partOrderEquipments)
			{
				order.AddEquipmentFromPartOrder(equipment);
			}
			
			order.UpdateRentsCount();
		}
	}
}
