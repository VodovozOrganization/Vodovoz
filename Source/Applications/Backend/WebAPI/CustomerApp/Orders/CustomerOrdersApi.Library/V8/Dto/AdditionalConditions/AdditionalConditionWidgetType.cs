using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.V8.Dto.AdditionalConditions
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
		CheckBox,
		/// <summary>
		/// Радио кнопка
		/// </summary>
		Radio
	}
}
