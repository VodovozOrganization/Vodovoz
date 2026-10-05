using System.Collections.Generic;
using CustomerOrders.Abstractions.V5.Sale;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using VodovozBusiness.Controllers;

namespace CustomerOrdersApi.Library.V5.Services
{
	/// <summary>
	/// Интерфейс работы со скидкой в онлайн заказе
	/// </summary>
	public interface IOnlineOrderDiscountHandler : IDiscountController
	{
		/// <summary>
		/// Применение промокода к онлайн заказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrderPromoCode">Данные, необходимые для проверки промокода и товары
		/// <see cref="IApplyingPromoCode"/></param>
		/// <returns></returns>
		Result<IEnumerable<IOnlineOrderedProduct>> TryApplyPromoCode(IUnitOfWork uow, IApplyingPromoCode onlineOrderPromoCode);
	}
}
