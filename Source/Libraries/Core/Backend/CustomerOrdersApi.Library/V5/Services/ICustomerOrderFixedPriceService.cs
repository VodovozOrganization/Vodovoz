using System.Collections.Generic;
using CustomerOrdersApi.Library.V5.Dto.Orders.FixedPrice;
using Vodovoz.Core.Domain.Results;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.V5.Services
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
		Result<IEnumerable<IOnlineOrderedProductWithFixedPrice>> ApplyFixedPriceToOnlineOrder(ApplyFixedPriceDto applyFixedPriceDto);
	}
}
