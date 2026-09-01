using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Errors.Orders;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Sale
{
	public class SaleDiscountController : DiscountController, ISaleDiscountController
	{
		public SaleDiscountController(
			ILogger<SaleDiscountController> logger,
			INomenclatureFixedPriceController fixedPriceController,
			IDiscountReasonRepository discountReasonRepository,
			IDiscountReasonSettings discountReasonSettings
			) : base(logger, discountReasonSettings)
		{
			FixedPriceController = fixedPriceController ?? throw new ArgumentNullException(nameof(fixedPriceController));
			DiscountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
		}

		protected INomenclatureFixedPriceController FixedPriceController { get; }
		protected IDiscountReasonRepository DiscountReasonRepository { get; }
		
		/// <inheritdoc/>
		public Result AddDiscountFromDiscountReason(
			DiscountReasonBase reason,
			IApplyDiscountReasonItem saleItem,
			bool isNotCheckPromoSetOrFixedPrice = false
			)
		{
			var canApplyResult = IsApplicableDiscount(reason, saleItem);
			
			if(canApplyResult.IsFailure)
			{
				return canApplyResult;
			}

			if(!isNotCheckPromoSetOrFixedPrice
				&& saleItem is OrderItem oi
				&& OrderItemContainsPromoSetOrFixedPrice(oi))
			{
				return Result.Failure(DiscountErrors.OrderItemContainsPromoSetOrFixedPrice);
			}

			return AddDiscount(reason, saleItem);
		}
		
		/// <summary>
		/// Содержит ли строка заказа промонабор или есть фикса
		/// </summary>
		/// <param name="orderItem">Строка заказа</param>
		/// <returns>true/false</returns>
		protected bool OrderItemContainsPromoSetOrFixedPrice(OrderItem orderItem)
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
					return FixedPriceController.ContainsFixedPrice(orderItem.Order.Client, orderItem.Nomenclature, orderItem.TotalCountInOrder);
				}
			}
			else
			{
				if(orderItem.Order.DeliveryPoint != null)
				{
					return FixedPriceController.ContainsFixedPrice(orderItem.Order.DeliveryPoint, orderItem.Nomenclature, orderItem.TotalCountInOrder);
				}
			}

			return false;
		}
		
		/// <inheritdoc/>
		public virtual void RemoveDiscount(int discountReasonId, IApplyDiscountReasonItem saleItem)
		{
			var discountReasons = saleItem.DiscountReasons;

			if(!discountReasons.Any())
			{
				return;
			}

			var reasonsToRemove = discountReasons
				.Where(r => r.Id == discountReasonId)
				.ToList();

			RemoveDiscountReasons(saleItem, reasonsToRemove);
			RecalculateTotalDiscount(saleItem);
		}

		public virtual void RecalculateDiscount(IDataContext context)
		{
			if(context.Data is not CommonRecalculateDiscount data)
			{
				throw new InvalidOperationException($"Передаваемый контекст для пересчета скидки должен быть {nameof(CommonRecalculateDiscount)}");
			}
			
			var saleItem = data.SaleItem;
			var newDiscount = data.DiscountValue;
			
			if(saleItem.CurrentCount == 0)
			{
				ClearDiscounts(saleItem);
			}
			else if(saleItem.DiscountReasons.Any())
			{
				RecalculateTotalDiscount(saleItem);
			}
			else
			{
				CalculateAndSetDiscount(
					saleItem,
					newDiscount ?? throw new InvalidOperationException("Не должно было сюда прийти пустого значения скидки"));
			}
		}

		/// <inheritdoc/>
		public Result SetCustomDiscount(
			IUnitOfWork uow,
			IApplyDiscountReasonItem saleItem,
			IDiscountValue receivedDiscountValue
		)
		{
			if(receivedDiscountValue.IsZeroDiscount)
			{
				ClearDiscounts(saleItem);
				return Result.Success();
			}
			
			var personalDiscountReasonId = PersonalDiscountReasonId;
			var personalDiscount = saleItem.PersonalDiscount;

			if(personalDiscount is null)
			{
				var personalDiscountReason = DiscountReasonRepository.GetDiscountReason(uow, personalDiscountReasonId);

				if(personalDiscountReason is null)
				{
					throw new InvalidOperationException(
						"В базе не найдено основание скидки Персональная скидка! Она необходима для установки индивидуальной скидки");
				}

				personalDiscount = PersonalDiscount.Create(personalDiscountReason, DiscountReasonSettings);
				
				var canApplyResult = IsApplicableDiscount(personalDiscountReason, saleItem);
				if(canApplyResult.IsFailure)
				{
					return canApplyResult;
				}
			}
			
			var totalDiscountValueFromReasons = CalculateTotalDiscountValueFromDiscountReasonsWithoutPersonalDiscount(saleItem);
			var newDiscountValue = ProcessPersonalDiscount(uow, saleItem, receivedDiscountValue, totalDiscountValueFromReasons, personalDiscount);
			
			SetDiscount(saleItem, newDiscountValue);
			return Result.Success();
		}
		
		public virtual bool IsDiscountValueCanBeAdded(IDiscountValue discountValue, IApplyDiscountReasonItem saleItem)
		{
			var isCalculateInPercent =
				saleItem.DiscountReasons.All(x => x.ValueType == DiscountUnits.percent)
				&& !discountValue.IsDiscountMoney;

			if(isCalculateInPercent)
			{
				var totalPercentDiscount = saleItem.DiscountReasons.Sum(x => x.Value) + discountValue.Discount;
				return totalPercentDiscount <= 100;
			}

			var alreadyAddedDiscount = CalculateTotalDiscount(saleItem);
			var discountMoneyToAdd = discountValue.IsDiscountMoney
				? discountValue.DiscountMoney
				: saleItem.CurrentRawPrice * discountValue.DiscountMoney / 100;

			return discountMoneyToAdd + alreadyAddedDiscount <= saleItem.CurrentRawPrice;
		}

		protected virtual void CalculateAndSetDiscount(IApplyDiscountReasonItem saleItem, IDiscountValue newDiscount)
		{
			var discountValue = CalculateDiscount(saleItem, newDiscount);
			saleItem.SetDiscount(discountValue);
		}

		/// <summary>
		/// Удаление скидок из позиции на продажу
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <param name="needSetDiscount">Нужно установить скидку</param>
		protected virtual void ClearDiscounts(IApplyDiscountReasonItem saleItem, bool needSetDiscount = true)
		{
			saleItem.DiscountReasons.Clear();
			saleItem.PersonalDiscount = null;
			saleItem.SetDiscount(DiscountValue.CreateZero());
		}
		
		/// <summary>
		/// Удаление скидок из позиций на продажу
		/// </summary>
		/// <param name="saleItems">Продаваемые позиции</param>
		protected virtual void ClearDiscounts(IEnumerable<IApplyDiscountReasonItem> saleItems)
		{
			foreach(var saleItem in saleItems)
			{
				ClearDiscounts(saleItem);
			}
		}

		/// <summary>
		/// Пересчет итоговой скидки из оснований скидок
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		protected virtual void RecalculateTotalDiscount(IApplyDiscountReasonItem saleItem)
		{
			var discountValue = CalculateTotalDiscountValueFromDiscountReasonsWithoutPersonalDiscount(saleItem);
			SetDiscount(saleItem, discountValue);
		}

		/// <summary>
		/// Установка скидки
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <param name="discountValue">Значение скидки <see cref="IDiscountValue"/></param>
		protected virtual void SetDiscount(IApplyDiscountReasonItem saleItem, IDiscountValue discountValue)
		{
			saleItem.SetDiscount(discountValue);
		}

		/// <summary>
		/// Подсчет итоговой скидки, включая персональную при наличии
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <returns>Итоговая скидка в деньгах</returns>
		protected virtual decimal CalculateTotalDiscount(IApplyDiscountReasonItem saleItem)
		{
			var totalDiscountInMoney = 0m;
			totalDiscountInMoney += CalculateTotalDiscountValueFromDiscountReasonsWithoutPersonalDiscount(saleItem)
				.DiscountMoney;

			if(saleItem.PersonalDiscount != null)
			{
				totalDiscountInMoney += saleItem.PersonalDiscount.DiscountValue.DiscountMoney;
			}
			
			return totalDiscountInMoney;
		}
		
		/// <summary>
		/// Установка скидки из основания скидки на конкретную позицию
		/// </summary>
		/// <param name="currentDiscounts">Текущие основания скидок</param>
		/// <param name="addingDiscount">Основание скидки</param>
		/// <param name="saleItem">Строка заказа</param>
		/// <param name="withoutRecalculate">Без пересчета скидки(актуально, когда нужно выполнить еще действия после добавления и потом пересчитать)</param>
		protected Result AddDiscount(
			DiscountReasonBase addingDiscount,
			IApplyDiscountReasonItem saleItem,
			bool withoutRecalculate = false
		)
		{
			var currentDiscounts = saleItem.DiscountReasons;
			
			try
			{
				if(addingDiscount != null && !IsDiscountReasonAdded(currentDiscounts, addingDiscount))
				{
					var totalDiscount = CalculateTotalDiscount(saleItem);
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
				Logger.LogError(ex, "При добавлении скидки произошла ошибка");
				return Result.Failure(DiscountErrors.AddDiscountException);
			}
		}
		
		protected bool IsDiscountReasonAdded(
			IEnumerable<DiscountReasonBase> currentDiscounts,
			DiscountReasonBase addingDiscount
		)
		{
			var foundDiscount = currentDiscounts.FirstOrDefault(x => x.Id == addingDiscount.Id);
			return foundDiscount != null;
		}
		
		private static IDiscountValue ProcessPersonalDiscount(
			IUnitOfWork uow,
			IApplyDiscountReasonItem saleItem,
			IDiscountValue receivedDiscountValue,
			IDiscountValue totalDiscountValueFromReasons,
			PersonalDiscount personalDiscount)
		{
			IDiscountValue newDiscountValue = null;
			
			if(receivedDiscountValue.IsDiscountMoney)
			{
				newDiscountValue = ProcessPersonalDiscount(
					uow,
					saleItem,
					receivedDiscountValue,
					totalDiscountValueFromReasons,
					receivedDiscountValue.DiscountMoney,
					totalDiscountValueFromReasons.DiscountMoney,
					personalDiscount);
			}
			else
			{
				newDiscountValue = ProcessPersonalDiscount(
					uow,
					saleItem,
					receivedDiscountValue,
					totalDiscountValueFromReasons,
					receivedDiscountValue.Discount,
					totalDiscountValueFromReasons.Discount,
					personalDiscount);
			}

			return newDiscountValue;
		}

		private static IDiscountValue ProcessPersonalDiscount(
			IUnitOfWork uow,
			IApplyDiscountReasonItem saleItem,
			IDiscountValue receivedDiscountValue,
			IDiscountValue totalDiscountValueFromReasons,
			decimal receivedDiscount,
			decimal totalDiscount,
			PersonalDiscount personalDiscount)
		{
			if(receivedDiscount > totalDiscount)
			{
				var differenceValue = receivedDiscount - totalDiscount;
				var newPersonalDiscountValue = CalculateDiscount(
					saleItem,
					DiscountValue.Create(receivedDiscountValue.IsDiscountMoney, differenceValue, differenceValue)
				);
				
				totalDiscountValueFromReasons.AddDiscountValue(newPersonalDiscountValue);
				personalDiscount.SetDiscount(newPersonalDiscountValue);

				if(personalDiscount.Id == 0)
				{
					saleItem.DiscountReasons.Add(personalDiscount.DiscountReason);
					saleItem.PersonalDiscount = personalDiscount;
					uow.Save(personalDiscount);
				}
			}
			else if(receivedDiscount <= totalDiscount)
			{
				if(personalDiscount.Id > 0)
				{
					saleItem.PersonalDiscount = null;
					saleItem.DiscountReasons.Remove(personalDiscount.DiscountReason);
				}
			}
			
			return totalDiscountValueFromReasons;
		}
		
		private static IDiscountValue CalculateDiscount(IApplyDiscountReasonItem saleItem, IDiscountValue newDiscount)
		{
			IDiscountValue discountValue = null;

			if(saleItem.CurrentRawPrice == 0 || newDiscount.IsZeroDiscount)
			{
				//TODO-5967 возможно стоит очищать все скидки при нуле ClearDiscounts
				discountValue = DiscountValue.CreateZero(newDiscount.IsDiscountMoney);
			}
			else if(newDiscount.IsDiscountMoney)
			{
				var discountMoney = newDiscount.DiscountMoney > saleItem.CurrentRawPrice
					? saleItem.CurrentRawPrice
					: newDiscount.DiscountMoney < 0
						? 0
						: newDiscount.DiscountMoney;
				
				var discountPercent = 100 * discountMoney / saleItem.CurrentRawPrice;
				
				discountValue = DiscountValue.Create(saleItem.DiscountData.IsDiscountMoney, discountPercent, discountMoney);
			}
			else
			{
				var discountPercent = newDiscount.Discount > 100
					? 100
					: newDiscount.Discount < 0
						? 0
						: newDiscount.Discount;
				
				var discountMoney = saleItem.CurrentRawPrice * discountPercent / 100;
				
				discountValue = DiscountValue.Create(saleItem.DiscountData.IsDiscountMoney, discountPercent, discountMoney);
			}

			return discountValue;
		}
		
		private void RemoveDiscountReasons(IApplyDiscountReasonItem saleItem, IList<DiscountReasonBase> discountReasons)
		{
			foreach(var reason in discountReasons)
			{
				discountReasons.Remove(reason);

				if(reason.Id == PersonalDiscountReasonId)
				{
					saleItem.PersonalDiscount = null;
				}
			}
		}
	}
}
