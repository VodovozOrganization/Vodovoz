using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Contracts.Extensions;
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
		/// Список интервалов отзвона перед доставкой
		/// </summary>
		public IEnumerable<CallBeforeArrivalMinutesDto> CallBeforeArrival { get; set; }
		/// <summary>
		/// Дополнительные параметры
		/// </summary>
		public IEnumerable<AdditionalCondition> Conditions { get; set; }

		public static DeliveryRulesConditions Create(
			IEnumerable<AdditionalCondition> conditions,
			bool isSelfDelivery)
		{
			var callBefore = isSelfDelivery
				? Array.Empty<CallBeforeArrivalMinutesDto>()
				: (
					from CallBeforeArrivalMinutesType item in Enum.GetValues(typeof(CallBeforeArrivalMinutesType))
					select CallBeforeArrivalMinutesDto.Create(
						item != CallBeforeArrivalMinutesType.DontCall
							? ((int)item).ToString()
							: item.ToString(),
						item.GetEnumDisplayName())
				)
				.ToArray();
			
			var deliveryConditions = new DeliveryRulesConditions
			{
				NeedDeliveryRulesRequest = !isSelfDelivery,
				Conditions = conditions,
				CallBeforeArrival = callBefore
			};

			return deliveryConditions;
		}
	}
}
