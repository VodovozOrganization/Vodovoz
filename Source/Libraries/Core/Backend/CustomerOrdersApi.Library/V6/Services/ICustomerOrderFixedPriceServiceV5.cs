using System.Collections.Generic;
using CustomerOrders.Contracts.V5.Orders.FixedPrice;
using CustomerOrders.Contracts.V5.Orders.OrderItem;
using Vodovoz.Core.Domain.Results;

namespace CustomerOrdersApi.Library.V6.Services
{
	/// <summary>
	/// Сервис по работе с фиксой в ИПЗ
	/// </summary>
	public interface ICustomerOrderFixedPriceServiceV5
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
		Result<IEnumerable<OnlineOrderItemWithFixedPriceV5>> ApplyFixedPriceToOnlineOrder(ApplyFixedPriceDto applyFixedPriceDto);
	}
}
