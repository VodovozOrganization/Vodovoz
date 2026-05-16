using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	/// <summary>
	/// Контракт получения правил доставки для заказа из корзины
	/// </summary>
	public interface IOnlineOrderFromCartDistrictRulesGetter : IOnlineOrderFromCartDeliveryPriceGetter
	{
		/// <summary>
		/// Получение правил доставки
		/// </summary>
		/// <param name="onlineOrder">Данные заказа</param>
		/// <returns></returns>
		Result<IEnumerable<DistrictRuleItemBase>> GetDeliveryRules(IOnlineOrderFromCart onlineOrder);
		/// <summary>
		/// Параметры онлайн заказа для расчета стоимости доставки
		/// </summary>
		OnlineOrderFromCartStateKey OnlineOrderFromCartStateKey { get; }
	}
}
