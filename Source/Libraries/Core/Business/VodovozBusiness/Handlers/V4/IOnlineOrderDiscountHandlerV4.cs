using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Orders.V4;
using Vodovoz.Core.Domain.Results;
using VodovozBusiness.Controllers;
using VodovozBusiness.Nodes.V4;

namespace Vodovoz.Handlers
{
	/// <summary>
	/// Интерфейс работы со скидкой в онлайн заказе
	/// </summary>
	public interface IOnlineOrderDiscountHandlerV4 : IDiscountController
	{
		/// <summary>
		/// Применение промокода к онлайн заказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrderPromoCode">Данные, необходимые для проверки промокода и товары
		/// <see cref="CanApplyOnlineOrderPromoCodeV4"/></param>
		/// <returns></returns>
		Result<IEnumerable<IOnlineOrderedProductV4>> TryApplyPromoCode(IUnitOfWork uow, CanApplyOnlineOrderPromoCodeV4 onlineOrderPromoCode);
	}
}
