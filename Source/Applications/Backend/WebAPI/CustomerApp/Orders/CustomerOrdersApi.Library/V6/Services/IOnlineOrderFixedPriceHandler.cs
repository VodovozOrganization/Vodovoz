using System.Collections.Generic;
using CustomerOrders.Abstractions.V6.Sale;
using CustomerOrdersApi.Library.V6.Dto.Orders.FixedPrice;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;

namespace CustomerOrdersApi.Library.V6.Services
{
	/// <summary>
	/// Интерфейс для работы с фиксой в ИПЗ
	/// </summary>
	public interface IOnlineOrderFixedPriceHandler
	{
		/// <summary>
		/// Применение фиксы к онлайн заказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="receivedData">Данные, необходимые для проверки фиксы и товары
		/// <see cref="IApplyingFixedPrice"/></param>
		/// <returns></returns>
		Result<IEnumerable<OnlineOrderItemWithFixedPriceDto>> TryApplyFixedPrice(
			IUnitOfWork uow, IApplyingFixedPrice receivedData);
	}
}
