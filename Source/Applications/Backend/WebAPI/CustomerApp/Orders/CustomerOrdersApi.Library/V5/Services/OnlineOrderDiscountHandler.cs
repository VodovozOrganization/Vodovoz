using System;
using System.Collections.Generic;
using System.Linq;
using CustomerApp.Contracts.Common;
using CustomerOrders.Abstractions;
using CustomerOrders.Abstractions.V5.Sale;
using CustomerOrdersApi.Library.V5.Factories;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Application.Sale;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Settings.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Extensions;

namespace CustomerOrdersApi.Library.V5.Services
{
	internal class OnlineOrderDiscountHandler : DiscountController, IOnlineOrderDiscountHandler
	{
		private readonly IDiscountReasonRepository _discountReasonRepository;
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
		/// <param name="onlineOrderPromoCode">Данные, необходимые для проверки промокода и товары
		/// <see cref="CanApplyOnlineOrderPromoCode"/></param>
		/// <returns></returns>
		public Result<IEnumerable<IOnlineOrderedProduct>> TryApplyPromoCode(IUnitOfWork uow, IApplyingPromoCode onlineOrderPromoCode)
		{
			var discountPromoCode = _discountReasonRepository.GetActivePromoCode(uow, onlineOrderPromoCode.PromoCode);
			var localTime = onlineOrderPromoCode.RequestTime.ToLocalTime();
			var date = localTime.Date;
			var time = localTime.TimeOfDay;
			var orderSum = GetOnlineOrderSum(onlineOrderPromoCode.OnlineOrderItems);

			if(discountPromoCode is null)
			{
				return Result.Failure<IEnumerable<IOnlineOrderedProduct>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.NotFound);
			}

			if(date.Date < discountPromoCode.StartDate || date.Date > discountPromoCode.EndDate)
			{
				return Result.Failure<IEnumerable<IOnlineOrderedProduct>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.ExpiredDateDuration);
			}

			if(time < discountPromoCode.StartTime || time > discountPromoCode.EndTime)
			{
				return Result.Failure<IEnumerable<IOnlineOrderedProduct>>(
					Vodovoz.Errors.Orders.DiscountErrors.PromoCode.ExpiredTimeDuration(
						discountPromoCode.StartTimePromoCodeString, discountPromoCode.EndTimePromoCodeString));
			}

			if(orderSum < discountPromoCode.OrderMinSum)
			{
				return Result.Failure<IEnumerable<IOnlineOrderedProduct>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.InvalidMinimalOrderSum);
			}

			if(discountPromoCode.IsOneTimePromoCode
				&& _discountReasonRepository.HasBeenUsagePromoCode(uow, onlineOrderPromoCode.ErpCounterpartyId, discountPromoCode.Id))
			{
				return Result.Failure<IEnumerable<IOnlineOrderedProduct>>(
					Vodovoz.Errors.Orders.DiscountErrors.PromoCode.UsageLimitHasBeenExceeded);
			}

			return TryApplyPromoCode(uow, onlineOrderPromoCode.Source, discountPromoCode, onlineOrderPromoCode.OnlineOrderItems);
		}

		private Result<IEnumerable<IOnlineOrderedProduct>> TryApplyPromoCode(
			IUnitOfWork uow,
			ExternalSource source,
			PromoCodeDiscount discountPromoCode,
			IEnumerable<IOnlineOrderedProduct> products)
		{
			var promoCodeApplied = false;
			
			foreach(var product in products)
			{
				var nomenclature = uow.GetById<Nomenclature>(product.NomenclatureId);
				promoCodeApplied |= TryApplyPromoCode(uow, source, discountPromoCode, nomenclature, product);
			}

			return promoCodeApplied
				? Result.Success(products)
				: Result.Failure<IEnumerable<IOnlineOrderedProduct>>(
					Vodovoz.Errors.Orders.DiscountErrors.UnsuitableItemsInCart);
		}

		private bool TryApplyPromoCode(
			IUnitOfWork uow,
			ExternalSource source,
			PromoCodeDiscount discountPromoCode,
			Nomenclature nomenclature,
			IOnlineOrderedProduct product)
		{
			if(!CanApplicableDiscount(uow, source, discountPromoCode, nomenclature, product))
			{
				return false;
			}

			ApplyPromoCode(discountPromoCode, product);

			return true;
		}

		/// <summary>
		/// Применима ли скидка к позиции онлайн заказа
		/// 1. Если номенклатура не известна - <c>false</c>
		/// 2. Если это промо набор - <c>false</c>
		/// 3. Если есть фикса - <c>false</c>
		/// 4. Если у товара уже есть скидка - <c>false</c>
		/// 5. Если скидка не применима к данной позиции - <c>false</c>
		/// 6. Если товар имеет скидку для продажи онлайн - <c>false</c>
		/// 7. Если цена или количество товара 0 - <c>false</c>
		/// Иначе - <c>true</c>
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="source">источник</param>
		/// <param name="discountPromoCode">Промокод</param>
		/// <param name="nomenclature">Номенклатура</param>
		/// <param name="product">Позиция из корзины</param>
		/// <returns></returns>
		private bool CanApplicableDiscount(
			IUnitOfWork uow,
			ExternalSource source,
			PromoCodeDiscount discountPromoCode,
			Nomenclature nomenclature,
			IOnlineOrderedProduct product)
		{
			if(nomenclature is null)
			{
				return false;
			}

			if(product.PromoSetId.HasValue)
			{
				return false;
			}

			if(product.IsFixedPrice)
			{
				return false;
			}

			if(product.Discount > 0)
			{
				return false;
			}

			var applicableDiscountItem = _applicablePromotionFactory.CreateApplicablePromotion(uow, product);
			var isApplicableResult = IsApplicableDiscount(discountPromoCode, applicableDiscountItem);
			if(isApplicableResult.IsFailure)
			{
				return false;
			}

			var onlineParameters = nomenclature.NomenclatureOnlineParameters
				.FirstOrDefault(x => x.Type == source.ToGoodsOnlineParameterType());

			var onlinePrice = onlineParameters?.GetOnlinePrice(product.Count);

			if(onlineParameters?.NomenclatureOnlineDiscount != null
				|| onlinePrice?.PriceWithoutDiscount != null)
			{
				return false;
			}
			
			return product.Count * product.Price != 0;
		}

		private void ApplyPromoCode(PromoCodeDiscount discountPromoCode, IOnlineOrderedProduct product)
		{
			product.DiscountReasonId = discountPromoCode.Id;
			product.IsDiscountInMoney = discountPromoCode.ValueType == DiscountUnits.money;

			if(!product.IsDiscountInMoney)
			{
				product.Discount = discountPromoCode.Value > 100 ? 100 : discountPromoCode.Value;
			}
			else
			{
				var itemSum = product.Price * product.Count;
				product.Discount = itemSum < discountPromoCode.Value ? itemSum : discountPromoCode.Value;
			}
		}

		private decimal GetOnlineOrderSum(IEnumerable<IOnlineOrderedProduct> products)
		{
			return products.Sum(x =>
				x.IsDiscountInMoney
					? x.Count * x.Price - x.Discount
					: x.Count * x.Price * (1 - x.Discount / 100)
			);
		}
	}
}
