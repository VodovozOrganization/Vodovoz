using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Core.Application.Sale;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Errors.Orders;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class OrderDiscountsController : DiscountController, IOrderDiscountsController
	{
		private readonly ILogger<OrderDiscountsController> _logger;
		private readonly INomenclatureFixedPriceController _fixedPriceController;

		public OrderDiscountsController(
			ILogger<OrderDiscountsController> logger,
			INomenclatureFixedPriceController fixedPriceController,
			IDiscountReasonRepository discountReasonRepository,
			IDiscountReasonSettings discountReasonSettings
			)
			: base(discountReasonRepository, discountReasonSettings)
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
		
		public void SetCustomDiscountForOrder(
			IUnitOfWork uow,
			DiscountReason reason,
			IDiscountValue discountValue,
			IEnumerable<IApplyDiscountReasonItem> saleItems)
		{
			foreach(var saleItem in saleItems)
			{
				SetCustomDiscountForOrderItem(uow, saleItem, reason, discountValue);
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

			ClearDiscounts(orderItem);
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
		
		public override void RecalculateDiscount(IDataContext context)
		{
			/*if(!CheckInitializedProperties())
			{
				return;
			}*/
			
			if(context.Data is not OrderRecalculateDiscount data)
			{
				throw new InvalidOperationException(
					$"Передаваемый контекст для пересчета скидки в заказе должен быть {nameof(OrderRecalculateDiscount)}");
			}
			
			var saleItem = data.SaleItem;
			var newDiscountValue = data.DiscountValue;

			if(saleItem.CurrentCount == 0)
			{
				if(data.OrderInUndeliveredStatus)
				{
					RemoveAndPreserveDiscount(saleItem);
				}
				else
				{
					ClearDiscounts(saleItem);
				}
			}
			else
			{
				/*var discount = IsDiscountInMoney
					? DiscountMoney
					: Discount;
					*/
				CalculateAndSetDiscount(saleItem, newDiscountValue);
			}
		}
		
		/// <summary>
		/// Удаляет текущие скидки и сохраняет их в <see cref="OriginalDiscountReasons"/>.
		/// Восстановить скидку можно методом <see cref="RestoreOriginalDiscount"/>.
		/// </summary>
		public void RemoveAndPreserveDiscount(IPreserveDiscount saleItem)
		{
			if(saleItem.DiscountData.DiscountMoney > 0)
			{
				saleItem.OriginalDiscountMoney = saleItem.DiscountData.DiscountMoney;
				saleItem.OriginalDiscount = saleItem.DiscountData.Discount;

				saleItem.OriginalDiscountReasons.Clear();
				foreach(var reason in saleItem.DiscountReasons)
				{
					saleItem.OriginalDiscountReasons.Add(reason);
				}
			}
			
			ClearDiscounts(saleItem);
		}
		
		public void RecalculateDiscountWithPreserveOrRestoreDiscount(IPreserveDiscount saleItem)
		{
			/*if(!CheckInitializedProperties())
			{
				return;
			}*/
			
			if(saleItem.CurrentCount == 0)
			{
				RemoveAndPreserveDiscount(saleItem);
			}
			else
			{
				RestoreOriginalDiscount(saleItem);
			}
		}

		public void TryRestoreOriginalDiscount(IPreserveDiscount saleItem)
		{
			if(!saleItem.OriginalDiscountMoney.HasValue && !saleItem.OriginalDiscount.HasValue)
			{
				return;
			}

			saleItem.SetDiscount(
				DiscountValue.Create(
					saleItem.DiscountData.IsDiscountMoney,
					saleItem.OriginalDiscountMoney ?? 0,
					saleItem.OriginalDiscount ?? 0));

			saleItem.DiscountReasons.Clear();
			foreach(var reason in saleItem.OriginalDiscountReasons)
			{
				saleItem.DiscountReasons.Add(reason);
			}

			saleItem.OriginalDiscountMoney = null;
			saleItem.OriginalDiscount = null;
			saleItem.OriginalDiscountReasons.Clear();
		}

		private void RestoreOriginalDiscount(IPreserveDiscount saleItem)
		{
			TryRestoreOriginalDiscount(saleItem);
			CalculateAndSetDiscount(saleItem, saleItem.DiscountData);
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
		/// <param name="uow">unit of work</param>
		/// <param name="reason">Основание скидки</param>
		/// <param name="discountValue">Значение скидки</param>
		/// <param name="saleItem">Строка заказа</param>
		private void SetCustomDiscountForOrderItem(
			IUnitOfWork uow,
			IApplyDiscountReasonItem saleItem,
			DiscountReason reason,
			IDiscountValue discountValue)
		{
			var canApplyResult = CanApplyDiscount(reason, saleItem);
			
			if(canApplyResult.IsFailure)
			{
				return;
			}

			ClearDiscounts(saleItem);
			var addingResult = AddDiscount(reason, saleItem, true);

			if(addingResult.IsFailure)
			{
				return;
			}
			
			SetCustomDiscount(uow, saleItem, discountValue);
		}

		/// <summary>
		/// Установка скидки из основания скидки на конкретную позицию
		/// </summary>
		/// <param name="currentDiscounts">Текущие основания скидок</param>
		/// <param name="addingDiscount">Основание скидки</param>
		/// <param name="saleItem">Строка заказа</param>
		/// <param name="withoutRecalculate">Без пересчета скидки(актуально, когда нужно выполнить еще действия после добавления и потом пересчитать)</param>
		private Result AddDiscount(
			DiscountReason addingDiscount,
			IApplyDiscountReasonItem saleItem,
			bool withoutRecalculate = false
			)
		{
			var currentDiscounts = saleItem.DiscountReasons;
			
			try
			{
				if(addingDiscount != null && !IsDiscountReasonAdded(currentDiscounts, addingDiscount))
				{
					currentDiscounts.Add(addingDiscount);
				}

				if(!withoutRecalculate)
				{
					RecalculateTotalDiscount(saleItem);
				}
				
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
		
		public void ClearOrdersItemDiscounts(IList<IApplyDiscountReasonItem> orderItems)
		{
			throw new NotImplementedException();
		}
	}
}
