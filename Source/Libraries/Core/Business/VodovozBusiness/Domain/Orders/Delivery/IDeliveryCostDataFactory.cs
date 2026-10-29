using System.Collections.Generic;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools.Orders;

namespace VodovozBusiness.Domain.Orders.Delivery
{
	/// <summary>
	/// Получение данных по доставке для сервиса правил доставки
	/// </summary>
	public interface IDeliveryCostDataFactory
	{
		/// <summary>
		/// Получение данных по доставке <see cref="IDeliveryCostData"/>
		/// </summary>
		/// <param name="districtRules">Правила доставки</param>
		/// <param name="waterCounts">Количество бутылей в корзине</param>
		/// <returns></returns>
		IDeliveryCostData CreateDeliveryCostData(IList<DistrictRuleItemBase> districtRules, IWaterCount waterCounts);

		/// <summary>
		/// Получение данных по бесплатной доставке <see cref="IDeliveryCostData"/>
		/// </summary>
		/// <returns></returns>
		IDeliveryCostData CreateFreeDeliveryCostData();
	}
}
