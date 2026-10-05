using System;
using System.Collections.Generic;
using System.Linq;
using Core.Infrastructure;
using CustomerOrdersApi.Library.V8.Dto.AdditionalConditions;

namespace CustomerOrdersApi.Library.V8.Dto.Carts
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
			bool isSelfDelivery)
		{
			var deliveryConditions = new DeliveryRulesConditions
			{
				NeedDeliveryRulesRequest = !isSelfDelivery,
				Conditions = conditions,
			};

			return deliveryConditions;
		}
	}
}
