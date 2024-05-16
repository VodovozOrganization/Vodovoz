using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Extensions;
using Vodovoz.Factories;
using Vodovoz.Settings.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OrderFromOnlineOrderCreator : IOrderFromOnlineOrderCreator
	{
		private readonly IOrderSettings _orderSettings;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly ICounterpartyContractRepository _counterpartyContractRepository;
		private readonly ICounterpartyContractFactory _counterpartyContractFactory;

		public OrderFromOnlineOrderCreator(
			IOrderSettings orderSettings,
			INomenclatureRepository nomenclatureRepository,
			ICounterpartyContractRepository counterpartyContractRepository,
			ICounterpartyContractFactory counterpartyContractFactory)
		{
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_counterpartyContractRepository =
				counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
			_counterpartyContractFactory =
				counterpartyContractFactory ?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
		}

		public Order CreateOrderFromOnlineOrder(IUnitOfWork uow, Employee orderCreator, OnlineOrder onlineOrder)
		{
			var order = new Order
			{
				UoW = uow
			};

			return FillOrderFromOnlineOrder(order, onlineOrder, orderCreator);
		}

		public Order FillOrderFromOnlineOrder(
			Order order,
			OnlineOrder onlineOrder,
			Employee employee = null,
			bool manualCreation = false)
		{
			var paymentFrom = onlineOrder.OnlinePaymentSource.HasValue
				? order.UoW.GetById<PaymentFrom>(
					onlineOrder.OnlinePaymentSource.Value.ConvertToPaymentFromId(_orderSettings))
				: null;

			if(employee != null)
			{
				order.Author = employee;
			}
			
			order.Client = onlineOrder.Counterparty;
			order.DeliveryPoint = onlineOrder.DeliveryPoint;
			order.DeliveryDate = onlineOrder.DeliveryDate;
			order.DeliverySchedule = onlineOrder.DeliverySchedule;
			order.SelfDelivery = onlineOrder.IsSelfDelivery;
			order.IsFastDelivery = onlineOrder.IsFastDelivery;
			order.PaymentType = onlineOrder.OnlineOrderPaymentType.ConvertToOrderPaymentType();
			order.BottlesReturn = onlineOrder.BottlesReturn;
			order.OnlineOrder = onlineOrder.OnlinePayment;
			order.PaymentByCardFrom = paymentFrom;
			order.Trifle = onlineOrder.Trifle;
			order.DontArriveBeforeInterval = onlineOrder.DontArriveBeforeInterval;

			if(!string.IsNullOrWhiteSpace(onlineOrder.OnlineOrderComment))
			{
				order.Comment = onlineOrder.OnlineOrderComment;
			}
			
			if(!order.SelfDelivery)
			{
				order.CallBeforeArrivalMinutes = onlineOrder.CallBeforeArrivalMinutes ?? 15;
				order.IsDoNotMakeCallBeforeArrival = false;
			}
			
			order.UpdateOrCreateContract(order.UoW, _counterpartyContractRepository, _counterpartyContractFactory);

			//TODO проверка доступности быстрой доставки, если заказ с быстрой доставкой
			//скорее всего достаточно будет одной проверки при подтверждении заказа
			
			AddOrderItems(order, onlineOrder.OnlineOrderItems, manualCreation);
			AddFreeRentPackages(order, onlineOrder.OnlineRentPackages);
			
			return order;
		}

		private void AddOrderItems(Order order, IEnumerable<OnlineOrderItem> onlineOrderItems, bool manualCreation = false)
		{
			AddNomenclaturesFromManualCreationOrder(order, onlineOrderItems);
		}

		private void AddNomenclaturesFromManualCreationOrder(Order order, IEnumerable<OnlineOrderItem> onlineOrderItems)
		{
			var onlineOrderPromoSets = onlineOrderItems
				.Where(x => x.PromoSet != null)
				.ToLookup(x => x.PromoSetId);

			var otherItems =
				onlineOrderItems
					.Where(x => x.PromoSet is null);

			AddPromoSetFromManualCreationOrder(order, onlineOrderPromoSets);
			AddOtherItemsFromManualCreationOrder(order, otherItems);
		}

		private void AddPromoSetFromManualCreationOrder(Order order, ILookup<int?, OnlineOrderItem> onlineOrderPromoSets)
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
				var promoSetCount = onlinePromoItemsCount % promoSetItemsCount;

				if(promoSetCount == default)
				{
					promoSetCount = 1;
					//добавить сообщение о несоответствии количества позиций в пришедшем промике и существующем
				}

				for(var i = 0; i < promoSetCount; i++)
				{
					foreach(var proSetItem in promoSet.PromotionalSetItems)
					{
						order.AddNomenclature(
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
		
		private void AddOtherItemsFromManualCreationOrder(Order order, IEnumerable<OnlineOrderItem> otherItems)
		{
			foreach(var onlineOrderItem in otherItems)
			{
				TryAddNomenclature(order, onlineOrderItem);
			}
		}
		
		private void AddNomenclaturesFromAutoCreationOrder(Order order, IEnumerable<OnlineOrderItem> onlineOrderItems)
		{
			foreach(var onlineOrderItem in onlineOrderItems)
			{
				if(onlineOrderItem.PromoSet != null)
				{
					order.AddNomenclature(
						onlineOrderItem.Nomenclature,
						onlineOrderItem.Count,
						onlineOrderItem.IsDiscountInMoney ? onlineOrderItem.MoneyDiscount : onlineOrderItem.PercentDiscount,
						onlineOrderItem.IsDiscountInMoney,
						null,
						onlineOrderItem.PromoSet);
				}
				else
				{
					TryAddNomenclature(order, onlineOrderItem);
				}
			}
		}

		private void TryAddNomenclature(Order order, Product onlineOrderItem)
		{
			if(onlineOrderItem.Nomenclature is null)
			{
				return;
			}

			order.AddNomenclature(onlineOrderItem.Nomenclature, onlineOrderItem.Count);
		}

		private void AddFreeRentPackages(Order order, IEnumerable<OnlineFreeRentPackage> onlineRentPackages)
		{
			foreach(var onlineRentPackage in onlineRentPackages)
			{
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
				
				order.AddFreeRent(rentPackage, anyNomenclature);
			}
		}
	}
}
