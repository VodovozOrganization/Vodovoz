using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Errors;
using Vodovoz.Nodes;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Handlers
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
		/// <see cref="CanApplyOnlineOrderPromoCode"/></param>
		/// <returns></returns>
		Result<IEnumerable<IOnlineOrderedProduct>> TryApplyPromoCode(IUnitOfWork uow, CanApplyOnlineOrderPromoCode onlineOrderPromoCode);
	}
}
