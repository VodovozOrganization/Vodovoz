using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Errors.Orders;
using Vodovoz.Settings.Nomenclature;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Domain.Service;
using VodovozBusiness.Extensions;
using VodovozBusiness.Factories;
using VodovozBusiness.Services.Orders;
using VodovozBusiness.Services.Sale;
using VodovozBusiness.Specifications.Sale;
using VodovozBusiness.Validation;

namespace Vodovoz.Core.Application.Sale
{
	public class OrderSaleHandler : SaleWithTaxHandler, IOrderSaleHandler
	{
		private readonly OrderSaleItemHandler _saleItemHandler;
		private readonly IOrderRepository _orderRepository;
		private readonly IOrderContractUpdater _contractUpdater;
		private Nomenclature _fastDeliveryNomenclature;

		public OrderSaleHandler(
			IOrderRepository orderRepository,
			IOrderContractUpdater contractUpdater,
			OrderSaleItemHandler saleItemHandler,
			IGoodsPriceCalculator goodsPriceCalculator,
			IDeliveryPriceService deliveryPriceService,
			IFixedPriceGetter fixedPriceGetter,
			IDeliveryRepository deliveryRepository,
			INomenclatureSettings nomenclatureSettings,
			INomenclatureRepository nomenclatureRepository,
			IAddNomenclatureToSaleValidator addNomenclatureToSaleValidator,
			IAddPromoSetValidator addPromoSetValidator,
			ISaleItemFactory saleItemFactory
			) : base(
				saleItemHandler,
				goodsPriceCalculator,
				deliveryPriceService,
				fixedPriceGetter,
				deliveryRepository,
				nomenclatureSettings,
				nomenclatureRepository,
				addNomenclatureToSaleValidator,
				addPromoSetValidator,
				saleItemFactory
			)
		{
			_contractUpdater = contractUpdater ?? throw new ArgumentNullException(nameof(contractUpdater));
			_saleItemHandler = saleItemHandler;
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
		}
		
		private Order Order => Source as Order
			?? throw new InvalidOperationException($"Что-то пошло не так. Не смогли привести источник к заказу в {nameof(OrderSaleHandler)}");
		
		public override void SetPrice(
			ISaleItem saleItem,
			(SaleItemPriceType PriceType, decimal Price) priceData)
		{
			UpdatePriceType(saleItem, priceData);
			var discountItem = saleItem.ToPreserveDiscount();
			
			_saleItemHandler.SetPrice(
				OrderRecalculateDiscount.CreateDataContext(discountItem, Order.IsUndeliveredStatus),
				priceData.Price);
		}
		
		public override void SetPriceForNewSaleItem(
			ISaleItem newItem,
			(SaleItemPriceType PriceType, decimal Price) priceData)
		{
			UpdatePriceType(newItem, priceData);
			var discountItem = newItem.ToPreserveDiscount();
			_saleItemHandler.SetPriceForNewSaleItem(
				OrderRecalculateDiscount.CreateDataContext(discountItem, Order.IsUndeliveredStatus),
				priceData.Price);
		}

		public override void RecalculatePrice(ISaleItem saleItem)
		{
			var orderSaleItem = saleItem.ToOrderSaleItem();
			
			if(orderSaleItem.IsUserPrice
				|| orderSaleItem.PromoSet != null
				|| Order.OrderStatus == OrderStatus.Closed
				|| orderSaleItem.CopiedFromUndelivery)
			{
				return;
			}

			SetPrice(
				saleItem,
				GoodsPriceCalculator.CalculateItemPrice(
					Source.SaleItems,
					Source.DeliveryPoint,
					Source.Counterparty,
					saleItem,
					Source.HasPermissionsForAlternativePrice
				)
			);
		}

		public virtual void SetActualCount(IOrderSaleItem saleItem, decimal? newValue)
		{
			saleItem.ActualCount = newValue;

			var discountItem = saleItem.ToPreserveDiscount();
			_saleItemHandler.RecalculateDiscounts(
				OrderRecalculateDiscount.CreateDataContext(discountItem, Order.IsUndeliveredStatus));
		}

		public void RestoreSaleItemsDiscountsAndCount(RouteListItemStatus newStatus)
		{
			foreach(var saleItem in Order.OrderItems)
			{
				RestoreOriginalDiscountFromRestoreOrder(saleItem);
				if(newStatus == RouteListItemStatus.EnRoute)
				{
					SetActualCount(saleItem, null);
				}
				else
				{
					PreserveActualCount(saleItem, true);
				}
			}
		}

		public virtual void SetActualCountWithPreserveOrRestoreDiscount(IOrderSaleItem saleItem, decimal? newValue)
		{
			saleItem.ActualCount = newValue;

			var discountItem = saleItem.ToPreserveDiscount();
			_saleItemHandler.RecalculateDiscountWithPreserveOrRestoreDiscount(discountItem);
		}

		public virtual void SetActualCountZero()
		{
			foreach(var saleItem in Order.OrderItems)
			{
				if(!saleItem.ActualCount.HasValue)
				{
					SetActualCountZero(saleItem);
				}
			}
		}

		public virtual void PreserveActualCount(bool ignoreHasValue = false)
		{
			foreach(var saleItem in Order.OrderItems)
			{
				PreserveActualCount(saleItem, ignoreHasValue);
			}
		}

		public virtual void SetDepositsActualCounts()
		{
			if(Order.OrderItems.All(x => x.Nomenclature.Id == 157))
			{
				foreach(var saleItem in Order.OrderItems)
				{
					SetActualCount(saleItem, saleItem.Count > 0 ? saleItem.Count : (saleItem.ActualCount ?? 0));
				}
			}
		}

		public virtual void SetActualCountZero(IOrderSaleItem saleItem)
		{
			SetActualCount(saleItem, 0m);
		}

		protected virtual void PreserveActualCount(IOrderSaleItem saleItem, bool ignoreHasValue = false)
		{
			if(!ignoreHasValue && saleItem.ActualCount.HasValue)
			{
				return;
			}

			SetActualCount(saleItem, saleItem.Count);
		}

		public virtual void RestoreOriginalDiscountFromRestoreOrder()
		{
			foreach(var saleItem in Order.OrderItems)
			{
				RestoreOriginalDiscountFromRestoreOrder(saleItem);
			}
		}
		
		protected virtual void RestoreOriginalDiscountFromRestoreOrder(IOrderSaleItem saleItem)
		{
			var discountItem = saleItem.ToPreserveDiscount();
			_saleItemHandler.TryRestoreOriginalDiscount(discountItem);
			SetActualCount(saleItem, null);
		}

		public virtual void SetCountWithRecalculateRents(IRecalculateRentCount saleItem, decimal count)
		{
			if(!SetCount(saleItem, count))
			{
				return;
			}
			
			UpdateRentsCount();
		}
		
		public virtual void SetRentCount(IRecalculateRentCount saleItem, int count)
		{
			if(!_saleItemHandler.SetRentCount(saleItem, count))
			{
				return;
			}
			
			UpdateRentsCount();
		}

		public virtual void UpdateRentsCount()
		{
			var orderRentalItems = Order.OrderItems
				.Where(x => x.OrderItemRentSubType != OrderItemRentSubType.None)
				.ToList();

			foreach(var saleItem in orderRentalItems)
			{
				/*if(!SaleItems.Contains(saleItem))
				{
					continue;
				}*/

				switch(saleItem.OrderItemRentSubType)
				{
					case OrderItemRentSubType.RentServiceItem:
						SetRentEquipmentCount(saleItem, Order.GetRentEquipmentTotalCountForServiceItem(saleItem));
						break;
					case OrderItemRentSubType.RentDepositItem:
						SetRentEquipmentCount(saleItem, Order.GetRentEquipmentTotalCountForDepositItem(saleItem));
						break;
				}
			}
		}

		public void CopyDiscounts(IUnitOfWork uow, IApplyDiscountReasonItem saleItem, IApplyDiscountReasonItem copyingSaleItem)
		{
			_saleItemHandler.CopyDiscounts(uow, saleItem, copyingSaleItem);
		}

		public void CopyOriginalDiscounts(IUnitOfWork uow, IApplyDiscountReasonItem saleItem, IPreserveDiscount copyingSaleItem)
		{
			_saleItemHandler.CopyOriginalDiscounts(uow, saleItem, copyingSaleItem);
		}

		protected virtual void SetRentEquipmentCount(IRecalculateRentCount saleItem, int newEquipmentCount)
		{
			saleItem.RentEquipmentCount = newEquipmentCount;
			var newCount = 0m;
			
			switch(saleItem.OrderItemRentSubType)
			{
				case OrderItemRentSubType.RentServiceItem:
					newCount = saleItem.RentCount * saleItem.RentEquipmentCount;
					break;
				case OrderItemRentSubType.RentDepositItem:
					newCount = saleItem.RentEquipmentCount;
					break;
				default:
					return;
			}
			
			SetCount(saleItem, newCount);
		}
		
		private void UpdatePriceType(ISaleItem saleItem, (SaleItemPriceType PriceType, decimal Price) priceData)
		{
			var priceByTotalCount = GoodsPriceCalculator
				.CalculateItemPrice(
					Order.OrderItems,
					Order.DeliveryPoint,
					Order.Client,
					saleItem,
					Source.HasPermissionsForAlternativePrice);

			UpdatePriceType(saleItem, priceData, priceByTotalCount, OrderSaleItemSpecification.Create(priceByTotalCount));
		}

		#region Работа с товарами/услугами

		public override void AddSaleItem(
			IUnitOfWork uow,
			ISaleItem saleItem,
			(SaleItemPriceType PriceType, decimal Price) priceData
		)
		{
			base.AddSaleItem(uow, saleItem, priceData);
			_contractUpdater.UpdateContract(uow, Order);
		}
		
		/// <summary>
		/// Добавить оборудование из выбранного предыдущего заказа.
		/// </summary>
		/// <param name="uow">IUnitOfWork</param>
		/// <param name="contractUpdater">Сервис обновления договора заказа</param>
		/// <param name="saleHandler">Обработчик действий с продажей</param>
		/// <param name="copiedSaleItem">Элемент заказа.</param>
		public virtual void AddNomenclatureForSaleFromPreviousOrder(
			IUnitOfWork uow,
			ISaleItem copiedSaleItem)
		{
			if(copiedSaleItem.Nomenclature.Category != NomenclatureCategory.additional)
			{
				return;
			}
			
			var newSaleItemData = NewOrderSaleItem.Create(
				copiedSaleItem.Nomenclature,
				copiedSaleItem.Count,
				(SaleItemPriceType.General, copiedSaleItem.Price)
				);
			
			var newSaleItem = SaleItemFactory.Create(Order, newSaleItemData);

			AddSaleItem(uow, newSaleItem, newSaleItemData.PriceData);
		}

		//TODO узнать про этот метод, действительно ли должны переносится только эти товары(вода и категория товары)
		public virtual void FillOrderItems(IUnitOfWork uow, Order fromOrder)
		{
			Order.ClearOrderItemsList();
			
			foreach(var copiedOrderItem in fromOrder.OrderItems)
			{
				switch(copiedOrderItem.Nomenclature.Category)
				{
					case NomenclatureCategory.additional:
						AddNomenclatureForSaleFromPreviousOrder(uow, copiedOrderItem);
						continue;
					case NomenclatureCategory.water:
						TryAddNomenclature(uow, copiedOrderItem.Nomenclature, copiedOrderItem.Count);
						continue;
					default:
						continue;
				}
			}
		}
		
		public override Result TryAddNomenclatureFromPromoSet(IUnitOfWork uow, PromotionalSet proSet)
		{
			if(Order.IsLoadedFrom1C)
			{
				return Result.Failure(OrderErrors.CantAddProductTo1COrder);
			}
			
			var result = base.TryAddNomenclatureFromPromoSet(uow, proSet);

			return result.IsFailure ? result : Result.Success();
		}
		
		public void ActivatePromotionalSet(IUnitOfWork uow, PromotionalSet proSet)
		{
			//TODO надо поменять алгоритм, сначала полная проверка возможности добавления промика, затем активация и добавление номенклатур из него
			//Добавление спец. действий промонабора
			foreach(var action in proSet.PromotionalSetActions)
			{
				action.Activate(Order);
			}
			
			//Добавление номенклатур из промонабора
			TryAddNomenclatureFromPromoSet(uow, proSet);

			Order.ObservablePromotionalSets.Add(proSet);
		}
		
		/// <summary>
		/// Добавление/удаление номенклатуры для вызова мастера в зависимости от типа адреса
		/// </summary>
		public virtual void UpdateMasterCallNomenclatureIfNeeded(IUnitOfWork unitOfWork)
		{
			var masterCallNomenclature = NomenclatureRepository.GetMasterCallNomenclature(unitOfWork);

			if(Order.OrderAddressType == OrderAddressType.Service
				&& !Order.SelfDelivery)
			{
				TryAddMasterCallItem(unitOfWork, masterCallNomenclature);
			}
			else
			{
				RemoveMasterCallItem(unitOfWork, masterCallNomenclature);
			}
		}

		private void TryAddMasterCallItem(IUnitOfWork uow, Nomenclature masterCallNomenclature)
		{
			if(Order.OrderItems.Any(x => x.Nomenclature.Id == masterCallNomenclature.Id))
			{
				return;
			}

			var canApplyAlternativePrice = Order.HasPermissionsForAlternativePrice
				&& masterCallNomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= 1);

			var priceData = (SaleItemPriceType.General, 0m);
			var newSaleItem = SaleItemFactory
				.Create(Order, NewOrderSaleItem.Create(masterCallNomenclature, 1, priceData));

			AddSaleItem(uow, newSaleItem, priceData);
		}

		public void AddFreeRentDepositItem(IUnitOfWork uow, FreeRentPackage freeRentPackage)
		{
			if(freeRentPackage is null)
			{
				throw new ArgumentNullException(nameof(freeRentPackage), "Бесплатная аренда не может быть null");
			}
			
			var saleItem = SaleItemFactory.CreateNewFreeRentDepositItem(uow, freeRentPackage);
			AddSaleItem(uow, saleItem, (SaleItemPriceType.General, freeRentPackage.Deposit));
		}

		public void AddDailyRentDepositItem(IUnitOfWork uow, PaidRentPackage paidRentPackage)
		{
			if(paidRentPackage is null)
			{
				throw new ArgumentNullException(nameof(paidRentPackage), "Платная аренда не может быть null");
			}
			
			var saleItem = SaleItemFactory.CreateNewDailyRentDepositItem(uow, paidRentPackage);
			AddSaleItem(uow, saleItem, (SaleItemPriceType.General, paidRentPackage.Deposit));
		}

		public void AddDailyRentServiceItem(IUnitOfWork uow, PaidRentPackage paidRentPackage)
		{
			if(paidRentPackage is null)
			{
				throw new ArgumentNullException(nameof(paidRentPackage), "Платная аренда не может быть null");
			}
			
			var saleItem = SaleItemFactory.CreateNewDailyRentServiceItem(uow, paidRentPackage);
			AddSaleItem(uow, saleItem, (SaleItemPriceType.General, paidRentPackage.PriceDaily));
		}

		public void AddNonFreeRentDepositItem(IUnitOfWork uow, PaidRentPackage paidRentPackage)
		{
			if(paidRentPackage is null)
			{
				throw new ArgumentNullException(nameof(paidRentPackage), "Платная аренда не может быть null");
			}
			
			var saleItem = SaleItemFactory.CreateNewNonFreeRentDepositItem(uow, paidRentPackage);
			AddSaleItem(uow, saleItem, (SaleItemPriceType.General, paidRentPackage.Deposit));
		}

		public void AddNonFreeRentServiceItem(IUnitOfWork uow, PaidRentPackage paidRentPackage)
		{
			if(paidRentPackage is null)
			{
				throw new ArgumentNullException(nameof(paidRentPackage), "Платная аренда не может быть null");
			}
			
			var saleItem = SaleItemFactory.CreateNewNonFreeRentServiceItem(uow, paidRentPackage);
			AddSaleItem(uow, saleItem, (SaleItemPriceType.General, paidRentPackage.PriceMonthly));
		}

		private void RemoveMasterCallItem(IUnitOfWork uow, Nomenclature masterCallNomenclature)
		{
			var masterCallItemToRemove =
				Order.ObservableOrderItems.SingleOrDefault(x => x.Nomenclature.Id == masterCallNomenclature.Id);

			RemoveSaleItem(uow, masterCallItemToRemove);
		}
		
		public virtual void RemoveItemFromClosingOrder(IUnitOfWork uow, ISaleItem saleItem)
		{
			if((saleItem.Count != 0 && saleItem.Price != 0)
				|| Order.OrderEquipments.Any(x => x.OrderItem == saleItem))
			{
				return;
			}

			RemoveItem(uow, saleItem);
		}

		public virtual void TryAddFastDelivery(IUnitOfWork uow)
		{
			var fastDeliveryNomenclature = GetFastDeliveryNomenclature(uow);
			
			if(Order.IsFastDelivery && Source.SaleItems.All(x => x.Nomenclature.Id != fastDeliveryNomenclature.Id))
			{
				var canApplyAlternativePrice = Source.HasPermissionsForAlternativePrice
					&& fastDeliveryNomenclature.AlternativeNomenclaturePrices.Any(x => x.MinCount <= 1);

				var priceData = fastDeliveryNomenclature.GetPrice(1, canApplyAlternativePrice);
				var newSaleItem = SaleItemFactory.Create(Order, NewOrderSaleItem.Create(fastDeliveryNomenclature, 1, priceData));

				AddSaleItem(uow, newSaleItem, priceData);
			}
		}
		
		public virtual void RemoveFastDelivery(IUnitOfWork uow)
		{
			var fastDeliveryNomenclature = GetFastDeliveryNomenclature(uow);
			var fastDeliveryItemToRemove =
				Source.SaleItems.SingleOrDefault(x => x.Nomenclature.Id == fastDeliveryNomenclature.Id);

			RemoveSaleItem(uow, fastDeliveryItemToRemove);
		}

		public override Result TryRemoveSaleItem(IUnitOfWork uow, ISaleItem saleItem)
		{
			var orderEquipment = Order.OrderEquipments.FirstOrDefault(x =>
				x.OrderRentDepositItem == saleItem || x.OrderRentServiceItem == saleItem);

			if(orderEquipment != null)
			{
				var existingRentDepositItem = orderEquipment.OrderRentDepositItem;
				var existingNonFreeRentServiceItem = orderEquipment.OrderRentServiceItem;

				if(existingRentDepositItem != null || existingNonFreeRentServiceItem != null)
				{
					return Result.Failure(OrderErrors.RemovingOrderItemWithLinkedEquipment(orderEquipment.FullNameString));
				}
			}

			var isMovedToNewOrder = _orderRepository.IsMovedToTheNewOrder(uow, saleItem.Id);
			
			if(isMovedToNewOrder)
			{
				return Result.Failure(OrderErrors.RemovingOrderItemTransferredToAnotherOrder);
			}

			return base.TryRemoveSaleItem(uow, saleItem);
		}

		protected override void RemoveSaleItem(IUnitOfWork uow, ISaleItem saleItem)
		{
			base.RemoveSaleItem(uow, saleItem);

			var masterCallNomenclatureId = NomenclatureSettings.MasterCallNomenclatureId;
			
			//Если была удалена последняя номенклатура "мастер" - переходит в стандартный тип адреса
			if(Source.SaleItems.All(x => !(x.IsMasterNomenclature && x.Nomenclature.Id != masterCallNomenclatureId))
				&& saleItem.IsMasterNomenclature
				&& saleItem.Nomenclature.Id != masterCallNomenclatureId)
			{
				Order.OrderAddressType = OrderAddressType.Delivery;
			}

			_contractUpdater.UpdateContract(uow, Order);
		}

		public virtual void RemoveEquipment(IUnitOfWork uow, OrderEquipment equipment)
		{
			var rentDepositOrderItem = equipment.OrderRentDepositItem;
			var rentServiceOrderItem = equipment.OrderRentServiceItem;
			var totalEquipmentCountForDeposit = 0;
			var totalEquipmentCountForService = 0;

			if(rentDepositOrderItem != null)
			{
				totalEquipmentCountForDeposit = Order.GetRentEquipmentTotalCountForDepositItem(rentDepositOrderItem);
			}
			if(rentServiceOrderItem != null)
			{
				totalEquipmentCountForService = Order.GetRentEquipmentTotalCountForServiceItem(rentServiceOrderItem);
			}

			if(totalEquipmentCountForDeposit == equipment.Count || totalEquipmentCountForService == equipment.Count)
			{
				Order.ObservableOrderEquipments.Remove(equipment);
				RemoveSaleItem(uow, rentDepositOrderItem);
				RemoveSaleItem(uow, rentServiceOrderItem);
			}
			else
			{
				Order.ObservableOrderEquipments.Remove(equipment);
				UpdateRentsCount();
			}

			Order.UpdateDocuments();
		}

		protected override void AfterRemoveSaleItem(ISaleItem item)
		{
			DeleteOrderEquipmentOnOrderItem(item as OrderItem);
			Order.UpdateDocuments();
		}
		
		private void DeleteOrderEquipmentOnOrderItem(OrderItem orderItem)
		{
			var orderEquipments = Order.ObservableOrderEquipments
				.Where(x => x.OrderItem == orderItem)
				.ToList();
			
			foreach(var orderEquipment in orderEquipments)
			{
				Order.ObservableOrderEquipments.Remove(orderEquipment);
			}
		}

		#endregion
		
		private Nomenclature GetFastDeliveryNomenclature(IUnitOfWork uow)
		{
			if(_fastDeliveryNomenclature is null)
			{
				_fastDeliveryNomenclature = NomenclatureRepository.GetFastDeliveryNomenclature(uow);
			}

			return _fastDeliveryNomenclature;
		}
	}
}
