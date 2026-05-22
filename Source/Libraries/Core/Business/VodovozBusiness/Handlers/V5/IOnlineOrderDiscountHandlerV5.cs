using System;
using System.Collections.Generic;
using CustomerOrders.Contracts;
using CustomerOrders.Contracts.V5.Orders.OrderItem;
using CustomerOrders.Contracts.V5.Orders.PromoCodes;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Handlers
{
	/// <summary>
	/// Интерфейс работы со скидкой в онлайн заказе
	/// </summary>
	public interface IOnlineOrderDiscountHandlerV5 : IDiscountController
	{
		/// <summary>
		/// Применение промокода к онлайн заказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrderPromoCode">Данные, необходимые для проверки промокода и товары
		/// <see cref="CanApplyOnlineOrderPromoCode"/></param>
		/// <returns></returns>
		Result<IEnumerable<OnlineOrderItemDto>> TryApplyPromoCode(IUnitOfWork uow, CanApplyOnlineOrderPromoCodeV5 onlineOrderPromoCode);
		/// <summary>
		/// Проверка применимости промокода с корзины
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="source">ИПЗ</param>
		/// <param name="counterpartyId">Идентификатор клиента</param>
		/// <param name="orderSum">Сумма заказа</param>
		/// <param name="dateTime">Дата и время запроса</param>
		/// <param name="product">Товар</param>
		/// <returns></returns>
		(bool? PromoCodeValid, bool DiscountApplicable) IsApplicableDiscount(
			IUnitOfWork uow,
			ExternalSource source,
			int? counterpartyId,
			decimal orderSum,
			DateTime dateTime,
			IGoods product);
	}
}
