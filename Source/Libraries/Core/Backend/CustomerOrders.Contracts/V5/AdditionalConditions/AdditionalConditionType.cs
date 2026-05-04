using System.Text.Json.Serialization;

namespace CustomerOrders.Contracts.V5.AdditionalConditions
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
		DontArriveBeforeInterval
	}
}
