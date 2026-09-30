using System.Collections.Generic;
using CustomerOrdersApi.Library.Default.Dto.Orders;
using Vodovoz.Core.Domain.Results;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.Default.Services
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
		Result<IEnumerable<IOnlineOrderedProduct>> ApplyPromoCodeToOnlineOrder(ApplyPromoCodeDto applyPromoCodeDto);
	}
}
