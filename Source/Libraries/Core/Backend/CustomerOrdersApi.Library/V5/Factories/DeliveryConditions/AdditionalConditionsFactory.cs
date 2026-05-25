using System.Collections.Generic;
using CustomerOrders.Contracts.V5.AdditionalConditions;

namespace CustomerOrdersApi.Library.V5.Factories.DeliveryConditions
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
				DontArriveBeforeInterval()
			};
		}
		
		/// <inheritdoc/>
		public IEnumerable<AdditionalCondition> CreateDefault()
		{
			return new List<AdditionalCondition>
			{
				CreateConfirmByPhone(false, true),
				DontArriveBeforeInterval()
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
	}
}
