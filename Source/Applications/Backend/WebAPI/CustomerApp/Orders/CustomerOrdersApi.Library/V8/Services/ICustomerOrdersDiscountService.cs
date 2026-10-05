using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerOrders.Abstractions;
using CustomerOrders.Abstractions.V8.Sale;
using CustomerOrders.Contracts.V8.Sale;
using CustomerOrdersApi.Library.V8.Dto.Orders;
using CustomerOrdersApi.Library.V8.Dto.Orders.Promotions.Discounts;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V8.Services
{
	/// <summary>
	/// Интерфейс работы со скидками в онлайн заказе
	/// </summary>
	public interface ICustomerOrdersDiscountService
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
		ISalePromotion ApplyPromoCodeToOnlineOrder(ApplyPromoCodeDto applyPromoCodeDto);
		
		/// <summary>
		/// Возвращает данные по доступности использования скидки на первый заказ для клиента
		/// </summary>
		/// <param name="source">Источник заказа</param>
		/// <param name="externalCounterpartyId">Внешний Id пользователя</param>
		/// <param name="erpCounterpartyId">Id пользователя в ДВ</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Данные с результатом проверки</returns>
		Task<FirstOrderDiscountConditionsDto> CanApplyFirstOrderDiscount(
			ExternalSource source,
			Guid? externalCounterpartyId,
			int? erpCounterpartyId,
			CancellationToken cancellationToken);
		
		/// <summary>
		/// Применение скидки на первый заказ
		/// Если скидка недоступна возвращается пришедший список товаров, с детализацией по скидкам, если они были в списке
		/// </summary>
		/// <param name="applyFirstOrderDiscountDto">Данные для применения скидки <see cref="ApplyFirstOrderDiscountDto"/></param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		Task<AppliedFirstOrderDiscountDto> ApplyFirstOrderDiscount(
			ApplyFirstOrderDiscountDto applyFirstOrderDiscountDto,
			CancellationToken cancellationToken);

		/// <summary>
		/// Применение скидки за автозаказ
		/// </summary>
		/// <param name="applyAutoOrderDiscount">Данные для работы</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns><see cref="ApplyAutoOrderDiscountResponse"/></returns>
		ISalePromotion ProcessAutoOrderDiscount(
			ApplyAutoOrderDiscountRequest applyAutoOrderDiscount,
			CancellationToken cancellationToken);

		/// <summary>
		/// Получение данных скидки за автозаказ
		/// </summary>
		/// <returns>Данные скидки <see cref="DiscountDto"/></returns>
		DiscountDto GetAutoOrderDiscount();
	}
}
