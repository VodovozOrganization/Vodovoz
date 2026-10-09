using System;
using System.Collections.Generic;
using System.Linq;
using CustomerApp.Contracts.Common;
using CustomerOrders.Abstractions;
using CustomerOrders.Abstractions.V7.Carts;
using CustomerOrders.Abstractions.V7.Sale;
using CustomerOrders.Contracts.V7.Orders.Items;
using CustomerOrders.Contracts.V7.Sale;
using CustomerOrdersApi.Library.V7.Factories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Sale;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Nodes;

namespace CustomerOrdersApi.Library.V7.Services
{
	internal class OnlineOrderDiscountHandler : DiscountController, IOnlineOrderDiscountHandler
	{
		private readonly IDiscountReasonRepository _discountReasonRepository;
		private readonly IDiscountReasonSettings _discountReasonSettings;
		private readonly IApplicablePromotionFactory _applicablePromotionFactory;

		public OnlineOrderDiscountHandler(
			ILogger<OnlineOrderDiscountHandler> logger,
			IDiscountReasonRepository discountReasonRepository,
			IDiscountReasonSettings discountReasonSettings,
			IApplicablePromotionFactory applicablePromotionFactory
			)
			: base(logger, discountReasonSettings)
		{
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
			_discountReasonSettings = discountReasonSettings ?? throw new ArgumentNullException(nameof(discountReasonSettings));
			_applicablePromotionFactory = applicablePromotionFactory ?? throw new ArgumentNullException(nameof(applicablePromotionFactory));
		}

		/// <summary>
		/// Применение промокода к онлайн заказу
		/// 1. Ищем промокод без учета регистра, если не нашли, возвращаем <see cref="Vodovoz.Errors.Orders.DiscountErrors.PromoCode.NotFound"/>
		/// 2. Смотрим срок действия промокода, если запрос пришел не в этот интервал, возвращаем
		/// <see cref="Vodovoz.Errors.Orders.DiscountErrors.PromoCode.ExpiredDateDuration"/>
		/// 3. Проверяем время действия промокода, если запрос пришел в другое время возвращаем
		/// <see cref="Vodovoz.Errors.Orders.DiscountErrors.PromoCode.ExpiredTimeDuration"/>
		/// 4. Проверяем сумму заказа, если она меньше установленной в промокоде, возвращаем
		/// <see cref="Vodovoz.Errors.Orders.DiscountErrors.PromoCode.InvalidMinimalOrderSum"/>
		/// 5. Если промокод одноразовый и клиент его уже использовал раньше, возвращаем
		/// <see cref="Vodovoz.Errors.Orders.DiscountErrors.PromoCode.UsageLimitHasBeenExceeded"/>
		/// Иначе пытаемся применить этот промокод к товарам онлайн заказа
		/// Если он не подходит ни под один товар, возвращаем
		/// <see cref="Vodovoz.Errors.Orders.DiscountErrors.PromoCode.UnsuitableItemsInCart"/>
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="receivedData">Данные, необходимые для проверки промокода и товары
		/// <see cref="IApplyingPromoCode"/></param>
		/// <returns></returns>
		public Result<(bool AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> CartItems)> TryApplyPromoCode(
			IUnitOfWork uow,
			IApplyingPromoCode receivedData)
		{
			var discountPromoCode = _discountReasonRepository.GetActivePromoCode(uow, receivedData.PromoCode);
			var localTime = receivedData.RequestTime.ToLocalTime();
			var date = localTime.Date;
			var time = localTime.TimeOfDay;

			if(discountPromoCode is null)
			{
				return Result.Failure<(bool AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> CartItems)>(
					Vodovoz.Errors.Orders.DiscountErrors.PromoCode.NotFound);
			}

			if(date.Date < discountPromoCode.StartDate || date.Date > discountPromoCode.EndDate)
			{
				return Result.Failure<(bool AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> CartItems)>(
					Vodovoz.Errors.Orders.DiscountErrors.PromoCode.ExpiredDateDuration);
			}

			if(time < discountPromoCode.StartTime || time > discountPromoCode.EndTime)
			{
				return Result.Failure<(bool AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> CartItems)>(
					Vodovoz.Errors.Orders.DiscountErrors.PromoCode.ExpiredTimeDuration(
						discountPromoCode.StartTimePromoCodeString, discountPromoCode.EndTimePromoCodeString));
			}

			if(receivedData.OrderSum < discountPromoCode.OrderMinSum)
			{
				return Result.Failure<(bool AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> CartItems)>(
					Vodovoz.Errors.Orders.DiscountErrors.PromoCode.InvalidMinimalOrderSum);
			}

			if(discountPromoCode.IsOneTimePromoCode
				&& _discountReasonRepository.HasBeenUsagePromoCode(uow, receivedData.ErpCounterpartyId, discountPromoCode.Id))
			{
				return Result.Failure<(bool AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> CartItems)>(
					Vodovoz.Errors.Orders.DiscountErrors.PromoCode.UsageLimitHasBeenExceeded);
			}

			return TryApplyPromoCode(uow, receivedData.Source, discountPromoCode, receivedData.OnlineOrderItems);
		}

		public void CalculateDiscount(
			IOrderedCartItemWithDiscountDetails receivedCartItem,
			IEnumerable<DiscountReasonBase> discountReasons
		)
		{
			var currentRawPrice = receivedCartItem.Count * receivedCartItem.Price;
			var calculatingTotalMoneyDiscountDto = CalculatingTotalMoneyDiscountNode.Create(
				currentRawPrice,
				discountReasons
			);
			
			CalculateDiscount(calculatingTotalMoneyDiscountDto, receivedCartItem);
		}

		public IEnumerable<IOrderedCartItemWithDiscountDetails> TryApplyFirstOrderDiscount(
			IUnitOfWork uow,
			IApplyingFirstOrderDiscount receivedData)
		{
			var firstOrderDiscount = _discountReasonRepository.GetDiscountReason(uow, _discountReasonSettings.FirstOnlineOrderDiscountReasonId);

			if(firstOrderDiscount is null)
			{
				throw new InvalidOperationException("В базе нет скидки на первый заказ или не настроен его идентификатор!");
			}
			
			var cartItemsWithDiscountDetails = new List<IOrderedCartItemWithDiscountDetails>();
			
			foreach(var cartItem in receivedData.CartItems)
			{
				var cartItemWithDiscountDetails = OnlineOrderItemWithDiscountDetailsDto.Create(cartItem);
				
				TryApplyFirstOrderDiscount(uow, firstOrderDiscount, cartItemWithDiscountDetails);
				cartItemsWithDiscountDetails.Add(cartItemWithDiscountDetails);
			}
			
			return cartItemsWithDiscountDetails;
		}

		public IEnumerable<IOrderedCartItemWithDiscountDetails> CalculateDiscounts(
			IUnitOfWork uow,
			IEnumerable<IOrderedCartItem> cartItems
			)
		{
			var cartItemsWithDiscountDetails = new List<IOrderedCartItemWithDiscountDetails>();
			
			foreach(var cartItem in cartItems)
			{
				var cartItemWithDiscountDetails = OnlineOrderItemWithDiscountDetailsDto.Create(cartItem);
				var applicableDiscountItem = _applicablePromotionFactory.CreateApplicablePromotion(uow, cartItemWithDiscountDetails);
				
				CalculateDiscount(cartItemWithDiscountDetails, applicableDiscountItem.DiscountReasons);
				cartItemsWithDiscountDetails.Add(cartItemWithDiscountDetails);
			}
			
			return cartItemsWithDiscountDetails;
		}

		/// <inheritdoc/>
		public virtual (decimal TotalDiscount, IDictionary<int, IDiscountAmount> DiscountDetails) CalculateTotalDiscountDetails(
			ICalculatingTotalMoneyDiscount saleItem
		)
		{
			if(saleItem is null)
			{
				throw new ArgumentNullException(
					nameof(saleItem),
					$"Продаваемая позиция должна реализовывать интерфейс {nameof(ICalculatingTotalMoneyDiscount)}");
			}

			var currentSumWithoutDiscount = saleItem.CurrentRawPrice;
			var discountAmounts = new Dictionary<int, IDiscountAmount>();
			var totalDiscountMoney = 0m;

			foreach(var discountReason in saleItem.DiscountReasons)
			{
				var discountMoney = CalculateMoneyDiscount(currentSumWithoutDiscount, discountReason);
				totalDiscountMoney += discountMoney;

				IDiscountAmount discountAmount;

				if(currentSumWithoutDiscount >= totalDiscountMoney)
				{
					discountAmount = DiscountAmount.Create(discountReason.Id, discountReason.ToString(), discountMoney);
				}
				else
				{
					var difference = totalDiscountMoney - currentSumWithoutDiscount;
					discountAmount = DiscountAmount.Create(
						discountReason.Id,
						discountReason.ToString(),
						difference >= discountMoney ? 0m : discountMoney - difference);
					totalDiscountMoney = currentSumWithoutDiscount;
				}

				discountAmounts.Add(discountAmount.Id, discountAmount);
			}

			if(saleItem.PersonalDiscount != null)
			{
				totalDiscountMoney += saleItem.PersonalDiscount.DiscountValue.DiscountMoney;

				var personalDiscountAmount = DiscountAmount.Create(
					saleItem.PersonalDiscount.DiscountReason.Id,
					saleItem.PersonalDiscount.DiscountReason.ToString(),
					saleItem.PersonalDiscount.DiscountValue.DiscountMoney);

				discountAmounts.Add(personalDiscountAmount.Id, personalDiscountAmount);
			}

			return (totalDiscountMoney, discountAmounts);
		}

		private bool TryApplyFirstOrderDiscount(
			IUnitOfWork uow,
			DiscountReasonBase discountReason,
			IOrderedCartItemWithDiscountDetails receivedCartItem)
		{
			if(!CanApplicableDiscount(uow, discountReason, receivedCartItem, out var applicableDiscountItem))
			{
				CalculateDiscount(receivedCartItem, applicableDiscountItem.DiscountReasons);
				return false;
			}

			ApplyDiscount(uow, discountReason, receivedCartItem);

			return true;
		}

		private Result<(bool AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> CartItems)> TryApplyPromoCode(
			IUnitOfWork uow,
			ExternalSource source,
			PromoCodeDiscount discountPromoCode,
			IEnumerable<IOrderedCartItem> cartItems)
		{
			var promoCodeApplied = false;
			var promoCodeAppliedToAllItems = true;
			var cartItemsWithDiscountDetails = new List<IOrderedCartItemWithDiscountDetails>();
			
			foreach(var cartItem in cartItems)
			{
				var cartItemWithDiscountDetails = OnlineOrderItemWithDiscountDetailsDto.Create(cartItem);
				
				var applied = TryApplyPromoCode(uow, source, discountPromoCode, cartItemWithDiscountDetails);
				promoCodeAppliedToAllItems &= applied;
				promoCodeApplied |= applied;
				cartItemsWithDiscountDetails.Add(cartItemWithDiscountDetails);
			}

			return promoCodeApplied
				? Result.Success((promoCodeAppliedToAllItems, cartItemsWithDiscountDetails.AsEnumerable()))
				: Result.Failure<(bool AppliedToAllItems, IEnumerable<IOrderedCartItemWithDiscountDetails> CartItems)>(
					Vodovoz.Errors.Orders.DiscountErrors.UnsuitableItemsInCart);
		}

		private bool TryApplyPromoCode(
			IUnitOfWork uow,
			ExternalSource source,
			PromoCodeDiscount discountPromoCode,
			IOrderedCartItemWithDiscountDetails receivedCartItem)
		{
			if(!CanApplicableDiscount(uow, discountPromoCode, receivedCartItem, out var applicableDiscountItem))
			{
				CalculateDiscount(receivedCartItem, applicableDiscountItem.DiscountReasons);
				return false;
			}

			ApplyDiscount(uow, discountPromoCode, receivedCartItem);

			return true;
		}

		/// <summary>
		/// Применима ли скидка к позиции онлайн заказа
		/// </summary>
		/// <param name="discountReason">Промокод</param>
		/// <param name="uow">unit of work</param>
		/// <param name="receivedCartItem">Данные позиции корзины</param>
		/// <param name="applicableDiscountItem">Данные позиции корзины преобразованные для проверки применимости промокода</param>
		/// <returns></returns>
		private bool CanApplicableDiscount(
			IUnitOfWork uow,
			DiscountReasonBase discountReason,
			IOrderedCartItemWithDiscountDetails receivedCartItem,
			out IApplicablePromotion applicableDiscountItem)
		{
			applicableDiscountItem = _applicablePromotionFactory.CreateApplicablePromotion(uow, receivedCartItem);
			
			if(!IsApplicableDiscount(discountReason, applicableDiscountItem).IsSuccess)
			{
				return false;
			}
			
			return true;
		}

		private void ApplyDiscount(
			IUnitOfWork uow,
			DiscountReasonBase discountReason,
			IOrderedCartItemWithDiscountDetails receivedCartItem
			)
		{
			var discountIds = new List<int>(receivedCartItem.Discounts
				.Select(x => x.Id)
				.ToArray()
			)
			{
				discountReason.Id
			};

			CalculateDiscount(uow, receivedCartItem, discountIds);
		}

		private void CalculateDiscount(
			IUnitOfWork uow,
			IOrderedCartItemWithDiscountDetails receivedCartItem,
			IEnumerable<int> discountIds
			)
		{
			var currentRawPrice = receivedCartItem.Count * receivedCartItem.Price;
			var calculatingTotalMoneyDiscountDto = CalculatingTotalMoneyDiscountNode.Create(
				currentRawPrice,
				_discountReasonRepository.GetDiscountReasons(uow, discountIds)
			);
			
			CalculateDiscount(calculatingTotalMoneyDiscountDto, receivedCartItem);
		}
		
		private void CalculateDiscount(
			ICalculatingTotalMoneyDiscount calculatingTotalMoneyDiscount,
			IOrderedCartItemWithDiscountDetails receivedCartItem
		)
		{
			receivedCartItem.PriceWithoutDiscount ??= receivedCartItem.CurrentPrice;
			
			var totalDiscountDetails = CalculateTotalDiscountDetails(calculatingTotalMoneyDiscount);

			receivedCartItem.CurrentSum = Math.Round(receivedCartItem.Count * receivedCartItem.Price - totalDiscountDetails.TotalDiscount, 2);
			receivedCartItem.CurrentPrice = Math.Round(receivedCartItem.CurrentSum / receivedCartItem.Count, 2);

			foreach(var discountAmount in receivedCartItem.Discounts)
			{
				if(totalDiscountDetails.DiscountDetails.TryGetValue(discountAmount.Id, out var calculated))
				{
					discountAmount.Update(calculated.Name, calculated.Amount);
					totalDiscountDetails.DiscountDetails.Remove(discountAmount.Id);
				}
			}

			//т.к. добавляемый промокод отсутствует в изначальном списке скидок, то пробегаемся по оставшимся и добавляем
			foreach(var keyPairValue in totalDiscountDetails.DiscountDetails)
			{
				receivedCartItem.Discounts.Add(keyPairValue.Value);
			}
		}
	}
}
