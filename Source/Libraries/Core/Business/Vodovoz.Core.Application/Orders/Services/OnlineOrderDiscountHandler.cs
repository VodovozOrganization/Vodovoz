using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;
using Vodovoz.Handlers;
using Vodovoz.Nodes;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Extensions;
using VodovozBusiness.Nodes;

namespace Vodovoz.Core.Application.Orders.Services
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

			return TryApplyPromoCode(uow, onlineOrderPromoCode.Source, discountPromoCode, onlineOrderPromoCode.Products);
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
		/// <see cref="CanApplyOnlineOrderPromoCode"/></param>
		/// <returns></returns>
		public Result<IEnumerable<IOrderedCartItem>> TryApplyPromoCodeV7(
			IUnitOfWork uow,
			CanApplyOnlineOrderPromoCodeV7 receivedData)
		{
			var discountPromoCode = _discountReasonRepository.GetActivePromoCode(uow, receivedData.PromoCode);
			var date = receivedData.Time.Date;
			var time = receivedData.Time.TimeOfDay;

			if(discountPromoCode is null)
			{
				return Result.Failure<IEnumerable<IOrderedCartItem>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.NotFound);
			}

			if(date.Date < discountPromoCode.StartDatePromoCode || date.Date > discountPromoCode.EndDatePromoCode)
			{
				return Result.Failure<IEnumerable<IOrderedCartItem>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.ExpiredDateDuration);
			}

			if(time < discountPromoCode.StartTimePromoCode || time > discountPromoCode.EndTimePromoCode)
			{
				return Result.Failure<IEnumerable<IOrderedCartItem>>(
					Vodovoz.Errors.Orders.DiscountErrors.PromoCode.ExpiredTimeDuration(
						discountPromoCode.StartTimePromoCodeString, discountPromoCode.EndTimePromoCodeString));
			}

			if(receivedData.OrderSum < discountPromoCode.PromoCodeOrderMinSum)
			{
				return Result.Failure<IEnumerable<IOrderedCartItem>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.InvalidMinimalOrderSum);
			}

			if(discountPromoCode.IsOneTimePromoCode
				&& _discountReasonRepository.HasBeenUsagePromoCode(uow, receivedData.CounterpartyId, discountPromoCode.Id))
			{
				return Result.Failure<IEnumerable<IOrderedCartItem>>(
					Vodovoz.Errors.Orders.DiscountErrors.PromoCode.UsageLimitHasBeenExceeded);
			}

			return TryApplyPromoCode(uow, receivedData.Source, discountPromoCode, receivedData.Products);
		}

		private Result<IEnumerable<IOnlineOrderedProduct>> TryApplyPromoCode(
			IUnitOfWork uow,
			Source source,
			DiscountReason discountPromoCode,
			IEnumerable<IOnlineOrderedProduct> products)
		{
			var promoCodeApplied = false;
			
			foreach(var product in products)
			{
				var nomenclature = uow.GetById<Nomenclature>(product.NomenclatureId);
				promoCodeApplied |= TryApplyPromoCode(source, discountPromoCode, nomenclature, product);
			}

			return promoCodeApplied
				? Result.Success(products)
				: Result.Failure<IEnumerable<IOnlineOrderedProduct>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.UnsuitableItemsInCart);
		}

		private bool TryApplyPromoCode(
			Source source,
			DiscountReason discountPromoCode,
			Nomenclature nomenclature,
			IOnlineOrderedProduct product)
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
			Source source,
			DiscountReason discountPromoCode,
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

			if(!IsApplicableDiscount(discountPromoCode, nomenclature))
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

		private void ApplyPromoCode(DiscountReason discountPromoCode, IOnlineOrderedProduct product)
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
		
		private Result<IEnumerable<IOrderedCartItem>> TryApplyPromoCode(
			IUnitOfWork uow,
			Source source,
			DiscountReason discountPromoCode,
			IEnumerable<IOrderedCartItem> cartItems)
		{
			var promoCodeApplied = false;
			
			foreach(var cartItem in cartItems)
			{
				promoCodeApplied |= TryApplyPromoCode(uow, source, discountPromoCode, cartItem);
			}

			return promoCodeApplied
				? Result.Success(cartItems)
				: Result.Failure<IEnumerable<IOrderedCartItem>>(Vodovoz.Errors.Orders.DiscountErrors.PromoCode.UnsuitableItemsInCart);
		}

		private bool TryApplyPromoCode(
			IUnitOfWork uow,
			Source source,
			DiscountReason discountPromoCode,
			IOrderedCartItem receivedCartItem)
		{
			if(!CanApplicableDiscount(uow, source, discountPromoCode, receivedCartItem))
			{
				return false;
			}

			ApplyPromoCode(uow, discountPromoCode, receivedCartItem);

			return true;
		}

		/// <summary>
		/// Применима ли скидка к позиции онлайн заказа
		/// 1. Если это промонабор или пакет аренды - <c>false</c>
		/// 2. Если номенклатура не известна - <c>false</c>
		/// 3. Если есть фикса - <c>false</c>
		/// 4. Если у товара уже есть скидка - <c>false</c>
		/// 5. Если скидка не применима к данной позиции - <c>false</c>
		/// 6. Если товар имеет скидку для продажи онлайн - <c>false</c>
		/// 7. Если цена или количество товара 0 - <c>false</c>
		/// Иначе - <c>true</c>
		/// </summary>
		/// <param name="source">источник</param>
		/// <param name="discountPromoCode">Промокод</param>
		/// <param name="uow">unit of work</param>
		/// <param name="receivedCartItem">Данные позиции корзины</param>
		/// <returns></returns>
		private bool CanApplicableDiscount(
			IUnitOfWork uow,
			Source source,
			DiscountReason discountPromoCode,
			IOrderedCartItem receivedCartItem)
		{
			if(receivedCartItem.ItemType is SaleItemType.PromoSet or SaleItemType.RentPackage)
			{
				return false;
			}
			
			var nomenclature = uow.GetById<Nomenclature>(receivedCartItem.ErpId);
			
			if(nomenclature is null)
			{
				return false;
			}

			if(receivedCartItem.IsFixedPrice)
			{
				return false;
			}

			if(receivedCartItem.DiscountIds != null && receivedCartItem.DiscountIds.Any())
			{
				return false;
			}

			if(!IsApplicableDiscount(discountPromoCode, nomenclature))
			{
				return false;
			}

			var onlineParameters = nomenclature.NomenclatureOnlineParameters
				.FirstOrDefault(x => x.Type == source.ToGoodsOnlineParameterType());

			var onlinePrice = onlineParameters?.GetOnlinePrice(receivedCartItem.Count);

			if(onlineParameters?.NomenclatureOnlineDiscount != null
				|| onlinePrice?.PriceWithoutDiscount != null)
			{
				return false;
			}
			
			return receivedCartItem.Count * receivedCartItem.Price != 0;
		}

		private void ApplyPromoCode(
			IUnitOfWork uow,
			DiscountReason discountPromoCode,
			IOrderedCartItem receivedCartItem
			)
		{
			receivedCartItem.DiscountIds.Add(discountPromoCode.Id);
			receivedCartItem.PriceWithoutDiscount ??= receivedCartItem.CurrentPrice;

			var currentRawPrice = receivedCartItem.Count * receivedCartItem.Price;
			var calculatingTotalMoneyDiscountDto = CalculatingTotalMoneyDiscountNode.Create(
				currentRawPrice,
				_discountReasonRepository.GetDiscountReasons(uow, receivedCartItem.DiscountIds)
				);
			var totalMoneyDiscount = CalculateTotalDiscountInMoneyFromAddedReasons(calculatingTotalMoneyDiscountDto);

			if(currentRawPrice <= totalMoneyDiscount)
			{
				totalMoneyDiscount = currentRawPrice;
			}

			//TODO вынести в бизнес правило используемое в сущностях
			receivedCartItem.CurrentSum = Math.Round(receivedCartItem.Count * receivedCartItem.Price - totalMoneyDiscount, 2);
			receivedCartItem.CurrentPrice = Math.Round(receivedCartItem.CurrentSum / receivedCartItem.Count, 2);
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
