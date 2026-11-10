using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.V8.Dto.Sale
{
	/// <summary>
	/// Причина недоступности расчёта бонусов
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum BonusUnavailableReason
	{
		/// <summary>
		/// Клиент не участвует в бонусной программе
		/// </summary>
		NotParticipant,

		/// <summary>
		/// Контрагент ещё не передан в Mindbox
		/// </summary>
		CustomerNotRegistered,

		/// <summary>
		/// Сервис расчёта бонусов временно недоступен
		/// </summary>
		ServiceUnavailable
	}
}
