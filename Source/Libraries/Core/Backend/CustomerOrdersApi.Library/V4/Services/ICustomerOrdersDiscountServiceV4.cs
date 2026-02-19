using System.Collections.Generic;
using CustomerOrdersApi.Library.V4.Dto.Orders;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Core.Domain.Results;

namespace CustomerOrdersApi.Library.V4.Services
{
	/// <summary>
	/// Интерфейс работы со скидками в онлайн заказе
	/// </summary>
	public interface ICustomerOrdersDiscountServiceV4
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
		Result<IEnumerable<IOnlineOrderedProduct>> ApplyPromoCodeToOnlineOrder(ApplyPromoCodeDto applyPromoCodeDto);
	}
}
