using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.V8.Dto.AdditionalConditions
{
	/// <summary>
	/// Тип доп условия
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum AdditionalConditionType
	{
		/// <summary>
		/// Подтверждение по телефону
		/// </summary>
		ConfirmOrderByPhone,
		/// <summary>
		/// Не приезжать раньше интервала
		/// </summary>
		DontArriveBeforeInterval,
		/// <summary>
		/// Позвонить перед доставкой
		/// </summary>
		CallBeforeInterval
	}
}
