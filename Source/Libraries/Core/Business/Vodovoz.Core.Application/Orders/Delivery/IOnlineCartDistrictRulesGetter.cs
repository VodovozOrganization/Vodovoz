using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools.Orders;
using VodovozBusiness.Domain.Orders.Cart;

namespace Vodovoz.Core.Application.Orders.Delivery
{
	/// <summary>
	/// Получение данных по правилам доставки
	/// </summary>
	public interface IOnlineCartDistrictRulesGetter
	{
		/// <summary>
		/// Количество воды в корзине
		/// </summary>
		IWaterCount CartWaterCounts { get; }
		/// <summary>
		/// Получение правил доставки
		/// </summary>
		/// <param name="context">Контекст с данными</param>
		/// <returns></returns>
		Result<IEnumerable<DistrictRuleItemBase>> GetDeliveryRules(IDeliveryRulesRequestContext context);
	}
}
