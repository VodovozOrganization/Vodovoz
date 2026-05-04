using System.Collections.Generic;
using CustomerOrders.Contracts.V5.AdditionalConditions;

namespace CustomerOrders.Contracts.V5.Carts
{
	/// <summary>
	/// Доп условия по доставке
	/// </summary>
	public sealed class DeliveryRulesConditions
	{
		/// <summary>
		/// Нужен запрос интервалов доставки
		/// </summary>
		public bool NeedDeliveryRulesRequest { get; set; }
		/// <summary>
		/// Дополнительные параметры
		/// </summary>
		public IEnumerable<AdditionalCondition> Conditions { get; set; }

		public static DeliveryRulesConditions Create(
			IEnumerable<AdditionalCondition> conditions,
			bool needDeliveryRulesRequest = true)
		{
			return new DeliveryRulesConditions
			{
				NeedDeliveryRulesRequest = needDeliveryRulesRequest,
				Conditions = conditions
			};
		}
	}
}
