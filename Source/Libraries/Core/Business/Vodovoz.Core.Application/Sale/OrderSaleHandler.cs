using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Service;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;
using VodovozBusiness.Extensions;
using VodovozBusiness.Specifications.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class OrderSaleHandler : SaleWithTaxHandler, IOrderSaleHandler
	{
		private readonly OrderSaleItemHandler _saleItemHandler;

		public OrderSaleHandler(
			OrderSaleItemHandler saleItemHandler,
			IGoodsPriceCalculator goodsPriceCalculator
			) : base(saleItemHandler, goodsPriceCalculator)
		{
			_saleItemHandler = saleItemHandler;
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
		
		public void SetPriceForNewSaleItem(
			IOrderSaleItem newItem,
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
	}
}
