using System;
using System.Collections.Generic;
using System.Linq;
using Core.Infrastructure;
using CustomerOrdersApi.Library.V8.Dto.AdditionalConditions;
using CustomerOrdersApi.Library.V8.Dto.Carts;

namespace CustomerOrdersApi.Library.V8.Factories.DeliveryConditions
{
	/// <inheritdoc/>
	public class AdditionalConditionsFactory : IAdditionalConditionsFactory
	{
		/// <inheritdoc/>
		public IEnumerable<AdditionalCondition> CreateForNewClient()
		{
			return new List<AdditionalCondition>
			{
				CreateConfirmByPhone(true, false),
				DontArriveBeforeInterval(),
				CallBeforeInterval()
			};
		}
		
		/// <inheritdoc/>
		public IEnumerable<AdditionalCondition> CreateDefault()
		{
			return new List<AdditionalCondition>
			{
				CreateConfirmByPhone(false, true),
				DontArriveBeforeInterval(),
				CallBeforeInterval()
			};
		}

		private AdditionalCondition CreateConfirmByPhone(bool isActive, bool editable)
		{
			return AdditionalCondition.Create(
				nameof(AdditionalConditionType.ConfirmOrderByPhone),
				"Подтвердить заказ по телефону",
				nameof(AdditionalConditionWidgetType.CheckBox),
				isActive,
				editable
			);
		}
		
		private AdditionalCondition DontArriveBeforeInterval(bool isActive = false, bool editable = true)
		{
			return AdditionalCondition.Create(
				nameof(AdditionalConditionType.DontArriveBeforeInterval),
				"Не приезжать ранее интервала доставки",
				nameof(AdditionalConditionWidgetType.CheckBox),
				isActive,
				editable
			);
		}
		
		private AdditionalCondition CallBeforeInterval(bool isActive = false, bool editable = true)
		{
			var parameters = Enum.GetValues<CallBeforeArrivalMinutesType>()
				.OrderBy(x => x)
				.Select(item =>
					AdditionalCondition.AdditionalParameter.Create(((int)item).ToString(), item.GetEnumDisplayName()))
				.ToList();

			return AdditionalCondition.Create(
				nameof(AdditionalConditionType.CallBeforeInterval),
				"Позвонить перед доставкой",
				nameof(AdditionalConditionWidgetType.Radio),
				isActive,
				editable,
				parameters
			);
		}
	}
}
