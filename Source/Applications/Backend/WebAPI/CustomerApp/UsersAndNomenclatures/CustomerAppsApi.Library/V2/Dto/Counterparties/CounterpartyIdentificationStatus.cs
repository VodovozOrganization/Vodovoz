using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Статус идентификации клиента
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum CounterpartyIdentificationStatus : short
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
		/// Успех
		/// </summary>
		Success = 1,
		/// <summary>
		/// Клиент зарегистрирован
		/// </summary>
		CounterpartyRegistered = 2,
		/// <summary>
		/// Клиент зарегистрирован без электронной почты
		/// </summary>
		CounterpartyRegisteredWithoutEmail = 3,
		/// <summary>
		/// Нужна ручная обработка
		/// </summary>
		NeedManualHandling = 4
	}
}
