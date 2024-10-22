using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.Extensions;
using Vodovoz.Factories;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Settings.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OrderFromOnlineOrderCreator : IOrderFromOnlineOrderCreator
	{
		private readonly IOrderSettings _orderSettings;
		private readonly INomenclatureRepository _nomenclatureRepository;
		private readonly ICounterpartyContractRepository _counterpartyContractRepository;
		private readonly ICounterpartyContractFactory _counterpartyContractFactory;
		private readonly INomenclatureSettings _nomenclatureSettings;

		public OrderFromOnlineOrderCreator(
			IOrderSettings orderSettings,
			INomenclatureRepository nomenclatureRepository,
			ICounterpartyContractRepository counterpartyContractRepository,
			ICounterpartyContractFactory counterpartyContractFactory,
			INomenclatureSettings nomenclatureSettings)
		{
			_orderSettings = orderSettings ?? throw new ArgumentNullException(nameof(orderSettings));
			_nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			_counterpartyContractRepository =
				counterpartyContractRepository ?? throw new ArgumentNullException(nameof(counterpartyContractRepository));
			_counterpartyContractFactory =
				counterpartyContractFactory ?? throw new ArgumentNullException(nameof(counterpartyContractFactory));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
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
			order.SelfDelivery = onlineOrder.IsSelfDelivery;
			order.DeliveryDate = onlineOrder.DeliveryDate;
			order.DeliverySchedule = onlineOrder.DeliverySchedule;
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
			
			order.UpdateOrCreateContract(order.UoW, _counterpartyContractRepository, _counterpartyContractFactory);

			if(order.Client != null)
			{
				if(order.Client.ReasonForLeaving == ReasonForLeaving.Unknown)
				{
					order.Client.ReasonForLeaving = ReasonForLeaving.ForOwnNeeds;
				}
				
				AddOrderItems(order, onlineOrder.OnlineOrderItems, manualCreation);
				AddFreeRentPackages(order, onlineOrder.OnlineRentPackages);
			}
			
			return order;
		}

		private void AddOrderItems(Order order, IEnumerable<OnlineOrderItem> onlineOrderItems, bool manualCreation = false)
		{
			AddNomenclatures(order, onlineOrderItems, manualCreation);
		}

		private void AddNomenclatures(Order order, IEnumerable<OnlineOrderItem> onlineOrderItems, bool manualCreation = false)
		{
			var onlineOrderPromoSets = onlineOrderItems
				.Where(x => x.PromoSet != null)
				.ToLookup(x => x.PromoSetId);

			var otherItems =
				onlineOrderItems
					.Where(x => x.PromoSet is null);

			TryAddPromoSets(order, onlineOrderPromoSets);

			if(manualCreation)
			{
				TryAddOtherItemsFromManualCreationOrder(order, otherItems);
			}
			else
			{
				TryAddOtherItemsFromAutoCreationOrder(order, otherItems);
			}
		}

		private void TryAddPromoSets(Order order, ILookup<int?, OnlineOrderItem> onlineOrderPromoSets)
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
		
		private void TryAddOtherItemsFromManualCreationOrder(Order order, IEnumerable<OnlineOrderItem> otherItems)
		{
			foreach(var onlineOrderItem in otherItems)
			{
				if(onlineOrderItem.Nomenclature is null)
				{
					continue;
				}
				
				if(_nomenclatureSettings.PaidDeliveryNomenclatureId == onlineOrderItem.Nomenclature.Id
					|| _nomenclatureSettings.FastDeliveryNomenclatureId == onlineOrderItem.Nomenclature.Id)
				{
					continue;
				}

				if(onlineOrderItem.OnlineOrderErrorState.HasValue
					&& onlineOrderItem.OnlineOrderErrorState == OnlineOrderErrorState.WrongDiscountParametersOrIsNotApplicable)
				{
					order.AddNomenclature(onlineOrderItem.Nomenclature, onlineOrderItem.Count);
				}
				else
				{
					if(onlineOrderItem.DiscountReason is null)
					{
						order.AddNomenclature(onlineOrderItem.Nomenclature, onlineOrderItem.Count);
					}
					else
					{
						order.AddNomenclature(
							onlineOrderItem.Nomenclature,
							onlineOrderItem.Count,
							onlineOrderItem.GetDiscount,
							onlineOrderItem.IsDiscountInMoney,
							onlineOrderItem.DiscountReason);
					}
				}
			}
		}
		
		private void TryAddOtherItemsFromAutoCreationOrder(Order order, IEnumerable<OnlineOrderItem> onlineOrderItems)
		{
			foreach(var onlineOrderItem in onlineOrderItems)
			{
				if(onlineOrderItem.Nomenclature is null)
				{
					continue;
				}
				
				order.AddNomenclature(
					onlineOrderItem.Nomenclature,
					onlineOrderItem.Count,
					onlineOrderItem.GetDiscount,
					onlineOrderItem.IsDiscountInMoney,
					onlineOrderItem.DiscountReason);
			}
		}

		private void AddFreeRentPackages(Order order, IEnumerable<OnlineFreeRentPackage> onlineRentPackages)
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
				
				order.AddFreeRent(rentPackage, anyNomenclature);
			}
		}
	}
}
