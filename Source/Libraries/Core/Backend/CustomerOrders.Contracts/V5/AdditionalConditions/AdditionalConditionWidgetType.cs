using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts.V5.AdditionalConditions
{
	/// <summary>
	/// Тип виджета для отображения доп условия
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum AdditionalConditionWidgetType
	{
		/// <summary>
		/// Чек бокс
		/// </summary>
		CheckBox
	}
}
