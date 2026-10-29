using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Статус обновления клиента
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum CounterpartyUpdateStatus
	{
		/// <summary>
		/// Ошибка
		/// </summary>
		Error = -1,
		/// <summary>
		/// Клиент не найден
		/// </summary>
		CounterpartyNotFound = 0,
		/// <summary>
		/// Клиент обновлен
		/// </summary>
		CounterpartyUpdated = 1
	}
}
