using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders.V5;
using Vodovoz.Core.Domain.Results;
using VodovozBusiness.Controllers;
using VodovozBusiness.Nodes.V5;

namespace Vodovoz.Handlers
{
	/// <summary>
	/// Интерфейс работы со скидкой в онлайн заказе
	/// </summary>
	public interface IOnlineOrderDiscountHandlerV5 : IDiscountController
	{
		/// <summary>
		/// Применение промокода к онлайн заказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrderPromoCode">Данные, необходимые для проверки промокода и товары
		/// <see cref="CanApplyOnlineOrderPromoCodeV5"/></param>
		/// <returns></returns>
		Result<IEnumerable<IOnlineOrderedProductV5>> TryApplyPromoCode(IUnitOfWork uow, CanApplyOnlineOrderPromoCodeV5 onlineOrderPromoCode);
	}
}
