using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrders.Contracts;
using CustomerOrders.Contracts.V5.Orders.OrderItem;
using CustomerOrders.Contracts.V5.Orders.PromoCodes;
using Vodovoz.Core.Domain.Results;

namespace CustomerOrdersApi.Library.V5.Services
{
	/// <summary>
	/// Интерфейс работы со скидками в онлайн заказе
	/// </summary>
	public interface ICustomerOrdersDiscountServiceV5
	{
		/// <summary>
		/// Проверка подписи на применение промокода
		/// </summary>
		/// <param name="applyPromoCodeDto">Данные запроса</param>
		/// <param name="generatedSignature">Сгенерированная подпись</param>
		/// <returns>
		/// Подпись валидна - <c>true</c>
		/// Подпись не валидна - <c>false</c></returns>
		bool ValidateApplyingPromoCodeSignature(ApplyPromoCodeDto applyPromoCodeDto, out string generatedSignature);
		/// <summary>
		/// Проверка подписи на вывод сообщения при применении промокода
		/// </summary>
		/// <param name="promoCodeWarningDto">Данные запроса</param>
		/// <param name="generatedSignature">Сгенерированная подпись</param>
		/// <returns>
		/// Подпись валидна - <c>true</c>
		/// Подпись не валидна - <c>false</c></returns>
		bool ValidatePromoCodeWarningSignature(PromoCodeWarningDto promoCodeWarningDto, out string generatedSignature);
		/// <summary>
		/// Применение промокода к онлайн заказу
		/// </summary>
		/// <param name="applyPromoCodeDto">Данные запроса</param>
		/// <returns>Список товаров</returns>
		Result<IEnumerable<OnlineOrderItemDto>> ApplyPromoCodeToOnlineOrder(ApplyPromoCodeDto applyPromoCodeDto);

		/// <summary>
		/// Возвращает данные по доступности использования скидки на первый заказ для клиента
		/// </summary>
		/// <param name="source">Источник заказа</param>
		/// <param name="externalCounterpartyId">Внешний Id пользователя</param>
		/// <param name="counterpartyErpId">Id пользователя в ДВ</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Данные с результатом проверки</returns>
		Task<FirstOrderDiscountConditionsDto> GetFirstOrderDiscountConditions(ExternalSource source, Guid externalCounterpartyId, int? counterpartyErpId, CancellationToken cancellationToken);
	}
}
