using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Vodovoz.Controllers;
using Vodovoz.Core.Application.Sale;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using Vodovoz.Errors.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class OrderDiscountsController : DiscountWithTaxController, IOrderDiscountsController
	{
		private readonly ILogger<OrderDiscountsController> _logger;
		private readonly INomenclatureFixedPriceController _fixedPriceController;

		public OrderDiscountsController(
			ILogger<OrderDiscountsController> logger,
			INomenclatureFixedPriceController fixedPriceController,
			SaleItemTaxHandler taxHandler
			) : base(taxHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_fixedPriceController = fixedPriceController ?? throw new ArgumentNullException(nameof(fixedPriceController));
		}

		public Result TryApplyDiscountForSaleItem(
			DiscountReason addingDiscount,
			IApplyDiscountReasonItem saleItem,
			bool isNotCheckPromoSetOrFixedPrice = false)
		{
			var canApplyResult = CanApplyDiscount(addingDiscount, saleItem);
			if(canApplyResult.IsFailure)
			{
				return canApplyResult;
			}

			//TODO задать вопрос по логике применения фиксы на промик и фиксу с правами
			if(!isNotCheckPromoSetOrFixedPrice
				&& saleItem is OrderItem oi
				&& OrderItemContainsPromoSetOrFixedPrice(oi))
			{
				return Result.Failure(DiscountErrors.OrderItemContainsPromoSetOrFixedPrice);
			}

			return AddDiscount(addingDiscount, saleItem);
		}

		/// <summary>
		/// Возможность применения скидки на продаваемую позицию
		/// </summary>
		/// <param name="addingDiscount">Основание скидки</param>
		/// <param name="saleItem">Строка заказа</param>
		/// <returns>true/false</returns>
		private Result CanApplyDiscount(
			DiscountReason addingDiscount,
			IApplyDiscountReasonItem saleItem)
		{
			if(saleItem.Price * saleItem.CurrentCount == 0m)
			{
				return Result.Failure(DiscountErrors.ZeroSaleItemSum);
			}
			
			return IsApplicableDiscount(addingDiscount, saleItem);
		}
		
		public void SetCustomDiscountForOrderItems(
			DiscountReason reason,
			decimal discount,
			DiscountUnits unit,
			IEnumerable<IApplyDiscountReasonItem> orderItems)
		{
			foreach(var item in orderItems)
			{
				SetCustomDiscountForOrderItem(reason, discount, unit, item);
			}
		}

		public void SetDiscountFromDiscountReasonForOrder(
			DiscountReason reason,
			IEnumerable<IApplyDiscountReasonItem> saleItems,
			bool canChangeDiscountValue,
			out string messages)
		{
			messages = null;
			var i = 0;

			foreach(var saleItem in saleItems)
			{
				SetDiscountFromDiscountReasonForOrderItem(reason, saleItem, canChangeDiscountValue, out string message);

				if(message != null)
				{
					messages += $"№{i + 1} {message}";
				}

				i++;
			}
		}

		public bool SetDiscountFromDiscountReasonForOrderItem(
			DiscountReason reason, IApplyDiscountReasonItem orderItem, bool canChangeDiscountValue, out string message)
		{
			message = null;
			
			var canApplyResult = CanApplyDiscount(reason, orderItem);
			
			if(canApplyResult.IsFailure)
			{
				return false;
			}
			
			if(!canChangeDiscountValue
				&& orderItem is OrderItem oi
				&& OrderItemContainsPromoSetOrFixedPrice(oi))
			{
				message = $"{orderItem.Nomenclature.Name}\n";
				return false;
			}

			ClearOrderItemDiscounts(orderItem);
			var addDiscountResult = AddDiscount(reason, orderItem);

			if(addDiscountResult.IsFailure)
			{
				var error = addDiscountResult.Errors.FirstOrDefault();
				message = $"{orderItem.Nomenclature.Name} - {error?.Message}\n";
				return false;
			}

			return true;
		}

		public Result AddDiscountFromDiscountReasonForOrderItem(
			DiscountReason reason,
			IApplyDiscountReasonItem orderItem,
			bool isNotCheckPromoSetOrFixedPrice = false)
		{
			var canApplyResult = CanApplyDiscount(reason, orderItem);
			
			if(canApplyResult.IsFailure)
			{
				return canApplyResult;
			}

			if(!isNotCheckPromoSetOrFixedPrice
				&& orderItem is OrderItem oi
				&& OrderItemContainsPromoSetOrFixedPrice(oi))
			{
				return Result.Failure(DiscountErrors.OrderItemContainsPromoSetOrFixedPrice);
			}

			return AddDiscount(reason, orderItem);
		}

		protected override void SetDiscount(IApplyDiscountReasonItem saleItem, IDiscountValue discountValue)
		{
			saleItem.SetDiscount(discountValue);
		}

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
		private void SetCustomDiscountForOrderItem(DiscountReason reason, decimal discount, DiscountUnits unit, IApplyDiscountReasonItem orderItem)
		{
			var canApplyResult = CanApplyDiscount(reason, orderItem);
			
			if(canApplyResult.IsFailure)
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
		private void SetCustomDiscount(
			DiscountReason reason,
			decimal discount,
			DiscountUnits unit,
			IApplyDiscountReasonItem orderItem)
		{
			AddDiscount(reason, orderItem);
		}

		/// <summary>
		/// Установка скидки из основания скидки на конкретную позицию
		/// </summary>
		/// <param name="currentDiscounts">Текущие основания скидок</param>
		/// <param name="addingDiscount">Основание скидки</param>
		/// <param name="saleItem">Строка заказа</param>
		private Result AddDiscount(
			DiscountReason addingDiscount,
			IApplyDiscountReasonItem saleItem
			)
		{
			var currentDiscounts = saleItem.DiscountReasons;
			
			try
			{
				if(addingDiscount != null && !IsDiscountReasonAdded(currentDiscounts, addingDiscount))
				{
					currentDiscounts.Add(addingDiscount);
				}

				RecalculateTotalDiscountFromReasons(saleItem);
				return Result.Success();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "При добавлении скидки произошла ошибка");
				return Result.Failure(DiscountErrors.AddDiscountException);
			}
		}

		private bool IsDiscountReasonAdded(
			IEnumerable<DiscountReason> currentDiscounts,
			DiscountReason addingDiscount
			)
		{
			var foundDiscount = currentDiscounts.FirstOrDefault(x => x.Id == addingDiscount.Id);
			return foundDiscount != null;
		}

		/// <summary>
		/// Удаление скидки из строки заказа
		/// </summary>
		/// <param name="orderItem">Строка заказа</param>
		private void ClearOrderItemDiscounts(IApplyDiscountReasonItem orderItem)
		{
			//orderItem.ClearDiscounts();
		}
		
		public void ClearOrdersItemDiscounts(IList<IApplyDiscountReasonItem> orderItems)
		{
			throw new NotImplementedException();
		}

		public void RemoveDiscountFromOrdersItem(DiscountReason discountReason, IApplyDiscountReasonItem orderItem)
		{
			var discountsToRemove = orderItem.DiscountReasons.Where(x => x.Id == discountReason.Id).ToList();
			if(discountsToRemove.Any())
			{
				foreach(var discount in discountsToRemove)
				{
					//orderItem.RemoveDiscount(discount.Id);
				}
			}
		}
	}
}
