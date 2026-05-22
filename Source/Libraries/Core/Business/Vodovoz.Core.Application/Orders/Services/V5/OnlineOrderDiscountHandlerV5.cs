using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Contracts;
using CustomerOrders.Contracts.V5.Orders.Discounts;
using CustomerOrders.Contracts.V5.Orders.OrderItem;
using CustomerOrders.Contracts.V5.Orders.PromoCodes;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Handlers;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Extensions;

namespace Vodovoz.Core.Application.Orders.Services.V5
{
	public class OnlineOrderDiscountHandlerV5 : DiscountController, IOnlineOrderDiscountHandlerV5
	{
		private readonly IDiscountReasonRepository _discountReasonRepository;

		public OnlineOrderDiscountHandlerV5(
			IDiscountReasonRepository discountReasonRepository)
		{
			_discountReasonRepository = discountReasonRepository ?? throw new ArgumentNullException(nameof(discountReasonRepository));
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
		public Result<IEnumerable<OnlineOrderItemDto>> TryApplyPromoCode(IUnitOfWork uow, CanApplyOnlineOrderPromoCodeV5 onlineOrderPromoCode)
		{
			var discountPromoCode = _discountReasonRepository.GetActivePromoCode(uow, onlineOrderPromoCode.PromoCode);
			var date = onlineOrderPromoCode.Time.Date;
			var time = onlineOrderPromoCode.Time.TimeOfDay;
			var orderSum = onlineOrderPromoCode.OrderSum;

			if(discountPromoCode is null)
			{
				return Result.Failure<IEnumerable<OnlineOrderItemDto>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.NotFound);
			}

			if(date.Date < discountPromoCode.StartDatePromoCode || date.Date > discountPromoCode.EndDatePromoCode)
			{
				return Result.Failure<IEnumerable<OnlineOrderItemDto>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.ExpiredDateDuration);
			}

			if(time < discountPromoCode.StartTimePromoCode || time > discountPromoCode.EndTimePromoCode)
			{
				return Result.Failure<IEnumerable<OnlineOrderItemDto>>(
					Vodovoz.Errors.Orders.DiscountErrors.PromoCode.ExpiredTimeDuration(
						discountPromoCode.StartTimePromoCodeString, discountPromoCode.EndTimePromoCodeString));
			}

			if(orderSum < discountPromoCode.PromoCodeOrderMinSum)
			{
				return Result.Failure<IEnumerable<OnlineOrderItemDto>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.InvalidMinimalOrderSum);
			}

			if(discountPromoCode.IsOneTimePromoCode
				&& _discountReasonRepository.HasBeenUsagePromoCode(uow, onlineOrderPromoCode.CounterpartyId, discountPromoCode.Id))
			{
				return Result.Failure<IEnumerable<OnlineOrderItemDto>>(
					Vodovoz.Errors.Orders.DiscountErrors.PromoCode.UsageLimitHasBeenExceeded);
			}

			return TryApplyPromoCode(uow, onlineOrderPromoCode.Source, discountPromoCode, onlineOrderPromoCode.Products);
		}

		public (bool? PromoCodeValid, bool DiscountApplicable) IsApplicableDiscount(
			IUnitOfWork uow,
			ExternalSource source,
			int? counterpartyId,
			decimal orderSum,
			DateTime dateTime,
			IGoods product)
		{
			var discount = product.DiscountReason != null
				? uow.GetById<DiscountReason>(product.DiscountReason.Id)
				: null;
			
			var date = dateTime.Date;
			var time = dateTime.TimeOfDay;
			var response = (PromoCodeValid: (bool?)null, DiscountApplicable: true);

			if(discount is null)
			{
				response.PromoCodeValid = false;
				response.DiscountApplicable = false;

				return response;
			}

			if(!discount.IsPromoCode)
			{
				response.PromoCodeValid = null;
			}
			else if(date.Date < discount.StartDatePromoCode || date.Date > discount.EndDatePromoCode)
			{
				response.PromoCodeValid = false;
			}
			else if(time < discount.StartTimePromoCode || time > discount.EndTimePromoCode)
			{
				response.PromoCodeValid = false;
			}
			else if(orderSum < discount.PromoCodeOrderMinSum)
			{
				response.PromoCodeValid = false;
			}
			else if(discount.IsOneTimePromoCode
				&& _discountReasonRepository.HasBeenUsagePromoCode(uow, counterpartyId ?? 0, discount.Id))
			{
				response.PromoCodeValid = false;
			}
			
			if(!CanApplicableDiscount(source, discount, product))
			{
				response.DiscountApplicable = false;
			}

			return response;
		}

		private Result<IEnumerable<OnlineOrderItemDto>> TryApplyPromoCode(
			IUnitOfWork uow,
			ExternalSource source,
			DiscountReason discountPromoCode,
			IEnumerable<OnlineOrderItemDto> products)
		{
			var promoCodeApplied = false;
			
			foreach(var product in products)
			{
				var nomenclature = uow.GetById<Nomenclature>(product.NomenclatureId);
				promoCodeApplied |= TryApplyPromoCode(source, discountPromoCode, nomenclature, product);
			}

			return promoCodeApplied
				? Result.Success(products)
				: Result.Failure<IEnumerable<OnlineOrderItemDto>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.UnsuitableItemsInCart);
		}

		private bool TryApplyPromoCode(
			ExternalSource source,
			DiscountReason discountPromoCode,
			Nomenclature nomenclature,
			OnlineOrderItemDto product)
		{
			if(!CanApplicableDiscount(source, discountPromoCode, nomenclature, product))
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
		/// <param name="source">источник</param>
		/// <param name="discountPromoCode"></param>
		/// <param name="nomenclature"></param>
		/// <param name="product"></param>
		/// <returns></returns>
		private bool CanApplicableDiscount(
			ExternalSource source,
			DiscountReason discountPromoCode,
			Nomenclature nomenclature,
			OnlineOrderItemDto product)
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

			if(product.Discounts.Any(x => x.Discount > 0))
			{
				return false;
			}

			if(!IsApplicableDiscount(discountPromoCode, nomenclature))
			{
				return false;
			}

			var onlineParameters = nomenclature.GetNomenclatureOnlineParameters(source.ToSource());
			var onlinePrice = onlineParameters?.GetOnlinePrice(product.Count);

			if(onlineParameters?.NomenclatureOnlineDiscount != null
				|| onlinePrice?.PriceWithoutDiscount != null)
			{
				return false;
			}
			
			return product.Count * product.Price != 0;
		}
		
		private bool CanApplicableDiscount(
			ExternalSource source,
			DiscountReason discountPromoCode,
			IGoods product)
		{
			var nomenclature = product.Nomenclature;
			
			if(nomenclature is null)
			{
				return false;
			}

			if(product.PromoSet != null)
			{
				return false;
			}

			if(product.IsFixedPrice)
			{
				return false;
			}

			//TODO уточнить, будет ли промокод применяться теперь при возможности нескольких скидок
			/*
			if(product.Discount > 0)
			{
				return false;
			}
			*/

			if(!IsApplicableDiscount(discountPromoCode, nomenclature))
			{
				return false;
			}

			var onlineParameters = nomenclature.GetNomenclatureOnlineParameters(source.ToSource());
			var onlinePrice = onlineParameters?.GetOnlinePrice(product.Count);

			if(onlineParameters?.NomenclatureOnlineDiscount != null
				|| onlinePrice?.PriceWithoutDiscount != null)
			{
				return false;
			}
			
			return product.Count * product.Price != 0;
		}

		private void ApplyPromoCode(DiscountReason discountPromoCode, OnlineOrderItemDto product)
		{
			//TODO сделать нормальную работу со скидками

			var firstDiscount = product.Discounts.FirstOrDefault();

			if(firstDiscount is null)
			{
				firstDiscount = new DiscountDto();
			}
			
			firstDiscount.DiscountReasonId = discountPromoCode.Id;
			firstDiscount.IsDiscountInMoney = discountPromoCode.ValueType == DiscountUnits.money;

			if(!firstDiscount.IsDiscountInMoney)
			{
				firstDiscount.Discount = discountPromoCode.Value > 100 ? 100 : discountPromoCode.Value;
			}
			else
			{
				var itemSum = product.Price * product.Count;
				firstDiscount.Discount = itemSum < discountPromoCode.Value ? itemSum : discountPromoCode.Value;
			}
		}
	}
}
