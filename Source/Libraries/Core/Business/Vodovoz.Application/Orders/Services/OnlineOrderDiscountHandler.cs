using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Errors;
using Vodovoz.Handlers;
using Vodovoz.Nodes;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OnlineOrderDiscountHandler : DiscountController, IOnlineOrderDiscountHandler
	{
		private readonly IDiscountReasonRepository _discountReasonRepository;

		public OnlineOrderDiscountHandler(
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
		public Result<IEnumerable<IOnlineOrderedProduct>> TryApplyPromoCode(IUnitOfWork uow, CanApplyOnlineOrderPromoCode onlineOrderPromoCode)
		{
			var discountPromoCode = _discountReasonRepository.GetActivePromoCode(uow, onlineOrderPromoCode.PromoCode);
			var date = onlineOrderPromoCode.Time.Date;
			var time = onlineOrderPromoCode.Time.TimeOfDay;
			var orderSum = GetOnlineOrderSum(onlineOrderPromoCode.Products);

			if(discountPromoCode is null)
			{
				return Result.Failure<IEnumerable<IOnlineOrderedProduct>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.NotFound);
			}

			if(date.Date < discountPromoCode.StartDatePromoCode || date.Date > discountPromoCode.EndDatePromoCode)
			{
				return Result.Failure<IEnumerable<IOnlineOrderedProduct>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.ExpiredDateDuration);
			}

			if(time < discountPromoCode.StartTimePromoCode || time > discountPromoCode.EndTimePromoCode)
			{
				return Result.Failure<IEnumerable<IOnlineOrderedProduct>>(
					Vodovoz.Errors.Orders.DiscountErrors.PromoCode.ExpiredTimeDuration(
						discountPromoCode.StartTimePromoCodeString, discountPromoCode.EndTimePromoCodeString));
			}

			if(orderSum < discountPromoCode.PromoCodeOrderMinSum)
			{
				return Result.Failure<IEnumerable<IOnlineOrderedProduct>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.InvalidMinimalOrderSum);
			}

			if(discountPromoCode.IsOneTimePromoCode
				&& _discountReasonRepository.HasBeenUsagePromoCode(uow, onlineOrderPromoCode.CounterpartyId, discountPromoCode.Id))
			{
				return Result.Failure<IEnumerable<IOnlineOrderedProduct>>(
					Vodovoz.Errors.Orders.DiscountErrors.PromoCode.UsageLimitHasBeenExceeded);
			}

			return TryApplyPromoCode(uow, discountPromoCode, onlineOrderPromoCode.Products);
		}

		private Result<IEnumerable<IOnlineOrderedProduct>> TryApplyPromoCode(
			IUnitOfWork uow, DiscountReason discountPromoCode, IEnumerable<IOnlineOrderedProduct> products)
		{
			var promoCodeApplied = false;
			
			foreach(var product in products)
			{
				product.ClearDiscount();
				var nomenclature = uow.GetById<Nomenclature>(product.NomenclatureId);
				promoCodeApplied |= TryApplyPromoCode(discountPromoCode, nomenclature, product);
			}

			return promoCodeApplied
				? Result.Success(products)
				: Result.Failure<IEnumerable<IOnlineOrderedProduct>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.UnsuitableItemsInCart);
		}

		private bool TryApplyPromoCode(
			DiscountReason discountPromoCode,
			Nomenclature nomenclature,
			IOnlineOrderedProduct product)
		{
			if(!CanApplicableDiscount(discountPromoCode, nomenclature, product))
			{
				return false;
			}

			ApplyPromoCode(discountPromoCode, nomenclature, product);

			return true;
		}

		/// <summary>
		/// Применима ли скидка к позиции онлайн заказа
		/// 1. Если номенклатура не известна - <c>false</c>
		/// 2. Если это промо набор - <c>false</c>
		/// 3. Если скидка не применима к данной позиции - <c>false</c>
		/// 4. Если установлена большая скидка - <c>false</c>
		/// 4. Если цена или количество товара 0 - <c>false</c>
		/// Иначе - <c>true</c>
		/// </summary>
		/// <param name="discountPromoCode"></param>
		/// <param name="nomenclature"></param>
		/// <param name="product"></param>
		/// <returns></returns>
		private bool CanApplicableDiscount(DiscountReason discountPromoCode, Nomenclature nomenclature, IOnlineOrderedProduct product)
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
				var nomenclaturePrice = nomenclature.GetPrice(product.Count);

				if(nomenclaturePrice == 0)
				{
					return false;
				}
				
				var withDiscountPromoCode = discountPromoCode.ValueType == DiscountUnits.money
					? Math.Round(nomenclaturePrice * product.Count - discountPromoCode.Value / product.Count, 2)
					: Math.Round(nomenclaturePrice * (100 - discountPromoCode.Value) / 100, 2);

				if(product.PriceWithDiscount < withDiscountPromoCode)
				{
					return false;
				}
			}

			if(!IsApplicableDiscount(discountPromoCode, nomenclature))
			{
				return false;
			}
			
			return product.Count * product.Price != 0;
		}

		private void ApplyPromoCode(DiscountReason discountPromoCode, Nomenclature nomenclature, IOnlineOrderedProduct product)
		{
			product.DiscountReasonId = discountPromoCode.Id;
			product.IsDiscountInMoney = discountPromoCode.ValueType == DiscountUnits.money;

			if(product.IsFixedPrice)
			{
				product.Price = nomenclature.GetPrice(product.Count);
				product.IsFixedPrice = false;
			}

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
					: x.Count * x.Price * (1 - x.Discount / 100));
		}
	}
}
