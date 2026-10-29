using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Статус регистрации клиента
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum CounterpartyRegistrationStatus : short
	{
		/// <summary>
		/// Ошибка
		/// </summary>
		Error = -1,
		/// <summary>
		/// Клиент зарегистрирован
		/// </summary>
		CounterpartyRegistered = 1,
		/// <summary>
		/// Клиент с таким идентификатором уже существует
		/// </summary>
		CounterpartyWithSameExternalIdExists = 2,
		/// <summary>
		/// Клиент с таким номером телефона уже существует
		/// </summary>
		CounterpartyWithSamePhoneNumberExists = 3
	}
}
