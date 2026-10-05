using CustomerOrders.Abstractions.V8.Sale;
using CustomerOrdersApi.Library.V8.Dto.Orders.FixedPrice;

namespace CustomerOrdersApi.Library.V8.Services
{
	/// <summary>
	/// Сервис по работе с фиксой в ИПЗ
	/// </summary>
	public interface ICustomerOrderFixedPriceService
	{
		/// <summary>
		/// Проверка подписи запроса
		/// </summary>
		/// <param name="applyFixedPriceDto">Данные для генерации проверочной подписи</param>
		/// <param name="generatedSignature">Сгенерированная подпись</param>
		/// <returns><c>true</c> - подпись валидна, <c>false</c> - подпись не валидна</returns>
		bool ValidateApplyingFixedPriceSignature(ApplyFixedPriceDto applyFixedPriceDto, out string generatedSignature);
		/// <summary>
		/// Применение фиксы
		/// </summary>
		/// <param name="applyFixedPriceDto">Данные для применения фиксы</param>
		/// <returns>Список товаров в случае, если есть фикса. Сообщение ошибки</returns>
		ISalePromotion ApplyFixedPriceToOnlineOrder(ApplyFixedPriceDto applyFixedPriceDto);
	}
}
