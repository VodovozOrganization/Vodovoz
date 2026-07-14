using System.Collections.Generic;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools.Orders;

namespace VodovozBusiness.Domain.Orders.Delivery
{
	public interface IDeliveryCostMessageFactory
	{
		/// <summary>
		/// Получение сообщения о том, сколько нужно доавить определенных бутылей для бесплатной доставки
		/// </summary>
		/// <param name="districtRules">Правила доставки</param>
		/// <param name="waterCounts">Количество бутылей в корзине</param>
		/// <returns></returns>
		string CreateDeliveryCostMessage(IList<DistrictRuleItemBase> districtRules, IWaterCount waterCounts);
	}
}
