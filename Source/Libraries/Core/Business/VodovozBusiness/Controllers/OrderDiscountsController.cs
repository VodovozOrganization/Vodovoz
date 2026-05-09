using System;
using System.Collections.Generic;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Errors.Orders;

namespace VodovozBusiness.Controllers
{
	public class OrderDiscountsController : DiscountController, IOrderDiscountsController
	{
		private readonly INomenclatureFixedPriceController _fixedPriceController;

		public OrderDiscountsController(INomenclatureFixedPriceController fixedPriceController)
		{
			_fixedPriceController = fixedPriceController ?? throw new ArgumentNullException(nameof(fixedPriceController));
		}

		public void ClearOrdersItemDiscounts(IList<OrderItem> orderItems)
		{
			foreach(var item in orderItems)
			{
				ClearOrderItemDiscounts(item);
			}
		}
		
		public void SetCustomDiscountForOrder(DiscountReason reason, decimal discount, DiscountUnits unit, IList<OrderItem> orderItems)
		{
			foreach(var item in orderItems)
			{
				SetCustomDiscountForOrderItem(reason, discount, unit, item);
			}
		}

		public void SetDiscountFromDiscountReasonForOrder(
			DiscountReason reason, IList<OrderItem> orderItems, bool canChangeDiscountValue, out string messages)
		{
			messages = null;
			
			for(var i = 0; i < orderItems.Count; i++)
			{
				SetDiscountFromDiscountReasonForOrderItem(reason, orderItems[i], canChangeDiscountValue, out string message);

				if(message != null)
				{
					messages += $"№{i + 1} {message}";
				}
			}
		}

		public bool SetDiscountFromDiscountReasonForOrderItem(
			DiscountReason reason, OrderItem orderItem, bool canChangeDiscountValue, out string message)
		{
			message = null;
			
			if(!CanSetDiscount(reason, orderItem))
			{
				return false;
			}
			
			if(!canChangeDiscountValue && OrderItemContainsPromoSetOrFixedPrice(orderItem))
			{
				message = $"{orderItem.Nomenclature.Name}\n";
				return false;
			}

			AddDiscount(reason, orderItem);
			return true;
		}

		public void SetDiscountFromDiscountReasonForOrderItemWithoutShipment(
			DiscountReason reason, OrderWithoutShipmentForAdvancePaymentItem orderItem)
		{
			if(!CanSetDiscountForOrderWithoutShipment(reason, orderItem))
			{
				return;
			}

			AddDiscount(reason, orderItem);
		}

		/// <summary>
		/// Возможность установки скидки на строку заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка заказа</param>
		/// <returns>true/false</returns>
		private bool CanSetDiscount(DiscountReason reason, OrderItem orderItem) =>
			IsApplicableDiscount(reason, orderItem.Nomenclature)
			&& orderItem.Price * orderItem.CurrentCount != default(decimal);

		/// <summary>
		/// Возможность установки скидки на строку счета без отгрузки на предоплату
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка счета без отгрузки на предоплату</param>
		/// <returns>true/false</returns>
		private bool CanSetDiscountForOrderWithoutShipment(DiscountReason reason, OrderWithoutShipmentForAdvancePaymentItem orderItem) =>
			IsApplicableDiscount(reason, orderItem.Nomenclature)
			&& orderItem.Price * orderItem.Count != default(decimal);

		/// <summary>
		/// Содержит ли строка заказа промонабор или есть фикса
		/// </summary>
		/// <param name="orderItem">Строка заказа</param>
		/// <returns>true/false</returns>
		private bool OrderItemContainsPromoSetOrFixedPrice(OrderItem orderItem)
		{
			if(orderItem == null)
			{
				throw new ArgumentNullException(nameof(orderItem));
			}
			
			if(orderItem.PromoSet != null)
			{
				return true;
			}

			if(orderItem.Order.SelfDelivery)
			{
				if(orderItem.Order.Client != null)
				{
					return _fixedPriceController.ContainsFixedPrice(orderItem.Order.Client, orderItem.Nomenclature, orderItem.TotalCountInOrder);
				}
			}
			else
			{
				if(orderItem.Order.DeliveryPoint != null)
				{
					return _fixedPriceController.ContainsFixedPrice(orderItem.Order.DeliveryPoint, orderItem.Nomenclature, orderItem.TotalCountInOrder);
				}
			}

			return false;
		}

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
			orderItem.AddDiscount(unit == DiscountUnits.money, discount, reason);
		}

		/// <summary>
		/// Установка скидки из основания скидки на конкретную позицию
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="item">Элемент, к которому применяется скидка</param>
		private Result AddDiscount(DiscountReason reason, IDiscount item)
		{
			try
			{
				item.AddDiscount(reason.ValueType == DiscountUnits.money, reason.Value, reason);
				return Result.Success();
			}
			catch(Exception ex)
			{
				return Result.Failure(DiscountErrors.CreateAddDiscountException(ex.Message));
			}
		}

		/// <summary>
		/// Удаление скидки из строки заказа
		/// </summary>
		/// <param name="orderItem">Строка заказа</param>
		private void ClearOrderItemDiscounts(OrderItem orderItem)
		{
			orderItem.RemoveDiscount();
		}
	}
}
