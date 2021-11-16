using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Controllers
{
	public class OrderDiscountsController : IOrderDiscountsController
	{
		private readonly INomenclatureFixedPriceProvider _fixedPriceProvider;

		public OrderDiscountsController(INomenclatureFixedPriceProvider fixedPriceProvider)
		{
			_fixedPriceProvider = fixedPriceProvider ?? throw new ArgumentNullException(nameof(fixedPriceProvider));
		}

		/// <summary>
		/// Возможность установки скидки на строку заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка заказа</param>
		/// <returns>true/false</returns>
		private bool CanSetDiscount(DiscountReason reason, OrderItem orderItem) =>
			!OrderItemContainsPromoSetOrFixedPrice(orderItem)
			&& ContainsProductGroup(
				orderItem.Nomenclature.ProductGroup, (reason ?? throw new ArgumentNullException(nameof(reason))).ProductGroups)
			&& orderItem.Price * orderItem.CurrentCount != default(decimal);

		/// <summary>
		/// Установка определенной скидки на строку заказа с прикреплением указанного основания скидки,
		/// после проверки возможности этого действия
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="discount">Скидка</param>
		/// <param name="unit">Скидка в процентах или рублях</param>
		/// <param name="orderItem">Строка заказа</param>
		private void SetCustomDiscountForOrderItem(DiscountReason reason, decimal discount, DiscountUnits unit, OrderItem orderItem)
		{
			if(!CanSetDiscount(reason, orderItem))
			{
				return;
			}

			SetCustomDiscount(reason, discount, unit, orderItem);
		}
		
		/// <summary>
		/// Установка определенной скидки на строку заказа с прикреплением указанного основания скидки
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="discount">Скидка</param>
		/// <param name="unit">Скидка в процентах или рублях</param>
		/// <param name="orderItem">Строка заказа</param>
		private void SetCustomDiscount(DiscountReason reason, decimal discount, DiscountUnits unit, OrderItem orderItem)
		{
			orderItem.IsDiscountInMoney = unit == DiscountUnits.money;
			orderItem.DiscountSetter = discount;
			orderItem.DiscountReason = reason;
		}
		
		/// <summary>
		/// Установка скидки из основания скидки на строку заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка заказа</param>
		private void SetDiscount(DiscountReason reason, OrderItem orderItem)
		{
			orderItem.IsDiscountInMoney = reason.ValueType == DiscountUnits.money;
			orderItem.DiscountSetter = reason.Value;
			orderItem.DiscountReason = reason;
		}

		/// <summary>
		/// Удаление скидки из строки заказа
		/// </summary>
		/// <param name="orderItem">Строка заказа</param>
		private void RemoveDiscountFromOrderItem(OrderItem orderItem)
		{
			if(orderItem.DiscountReason == null)
			{
				return;
			}
			
			orderItem.DiscountReason = null;
			orderItem.IsDiscountInMoney = false;
			orderItem.DiscountMoney = default(decimal);
			orderItem.Discount = default(decimal);
		}

		/// <summary>
		/// Удаление скидки из заказа
		/// </summary>
		public void RemoveDiscountFromOrder(IList<OrderItem> orderItems)
		{
			foreach(var item in orderItems)
			{
				RemoveDiscountFromOrderItem(item);
			}
		}

		/// <summary>
		/// Устанавливает основание скидки с введенными значениями в рублях или процентах для всего заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="discount">Скидки</param>
		/// <param name="unit">Скидка в процентах или рублях</param>
		/// <param name="orderItems">Список строк заказа</param>
		public void SetCustomDiscountForOrder(DiscountReason reason, decimal discount, DiscountUnits unit, IList<OrderItem> orderItems)
		{
			foreach(var item in orderItems)
			{
				SetCustomDiscountForOrderItem(reason, discount, unit, item);
			}
		}

		/// <summary>
		/// Установка скидки исходя из выбранного основания скидки для всего заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItems">Список строк заказа</param>
		public void SetDiscountFromDiscountReasonForOrder(DiscountReason reason, IList<OrderItem> orderItems)
		{
			foreach(var item in orderItems)
			{
				SetDiscountFromDiscountReasonForOrderItem(reason, item);
			}
		}

		/// <summary>
		/// Установка скидки исходя из выбранного основания скидки для строки заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка заказа</param>
		public void SetDiscountFromDiscountReasonForOrderItem(DiscountReason reason, OrderItem orderItem)
		{
			if(!CanSetDiscount(reason, orderItem))
			{
				return;
			}

			SetDiscount(reason, orderItem);
		}

		/// <summary>
		/// Содержит ли строка заказа промонабор или есть фикса
		/// </summary>
		/// <param name="orderItem">Строка заказа</param>
		/// <returns>true/false</returns>
		public bool OrderItemContainsPromoSetOrFixedPrice(OrderItem orderItem)
		{
			if(orderItem.PromoSet != null)
			{
				return true;
			}

			if(orderItem.Order.SelfDelivery)
			{
				if(orderItem.Order.Client != null)
				{
					return _fixedPriceProvider.ContainsFixedPrice(orderItem.Order.Client, orderItem.Nomenclature);
				}
			}
			else
			{
				if(orderItem.Order.DeliveryPoint != null)
				{
					return _fixedPriceProvider.ContainsFixedPrice(orderItem.Order.DeliveryPoint, orderItem.Nomenclature);
				}
			}

			return false;
		}

		/// <summary>
		/// Содержит ли основание скидки в списке товарную группу строки заказа 
		/// </summary>
		/// <param name="itemProductGroup">Товарная группа строки заказа</param>
		/// <param name="discountProductGroups">Товарные группы основания скидки</param>
		/// <returns>true/false</returns>
		public bool ContainsProductGroup(ProductGroup itemProductGroup, IList<ProductGroup> discountProductGroups)
		{
			return itemProductGroup != null
				&& discountProductGroups.Any(discountProductGroup => CheckProductGroup(itemProductGroup, discountProductGroup));
		}

		/// <summary>
		/// Проверяет соответствие товарных групп у основания скидки и строки заказа,
		/// с обходом всех ее родительских групп
		/// </summary>
		/// <param name="itemProductGroup">Товарная группа строки заказа</param>
		/// <param name="discountProductGroup">Товарная группа основания скидки</param>
		/// <returns>true/false</returns>
		private bool CheckProductGroup(ProductGroup itemProductGroup, ProductGroup discountProductGroup)
		{
			while(true)
			{
				if(itemProductGroup == discountProductGroup)
				{
					return true;
				}

				if(itemProductGroup.Parent != null)
				{
					itemProductGroup = itemProductGroup.Parent;
					continue;
				}

				return false;
			}
		}
	}
}
