using System.Collections.Generic;
using CustomerOrders.Contracts.Interfaces;
using CustomerOrders.Contracts.V4.Orders.FixedPrice;
using Vodovoz.Core.Domain.Results;

namespace CustomerOrdersApi.Library.V4.Services
{
	/// <summary>
	/// Сервис по работе с фиксой в ИПЗ
	/// </summary>
	public interface ICustomerOrderFixedPriceServiceV4
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
		Result<IEnumerable<IOnlineOrderedProductWithFixedPriceV4>> ApplyFixedPriceToOnlineOrder(ApplyFixedPriceDto applyFixedPriceDto);
	}
}
