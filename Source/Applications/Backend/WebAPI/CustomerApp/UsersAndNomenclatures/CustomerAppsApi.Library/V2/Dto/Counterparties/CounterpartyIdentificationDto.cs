using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Информация об идентификации клиента
	/// </summary>
	public class CounterpartyIdentificationDto
	{
		/// <summary>
		/// Данные о зарегистрированном физ лице
		/// </summary>
		public RegisteredNaturalCounterpartyDto RegisteredNaturalCounterpartyDto { get; set; }
		/// <summary>
		/// Описание ошибки, если есть
		/// </summary>
		public string ErrorDescription { get; set; }
		/// <summary>
		/// Статус идентификации
		/// </summary>
		[JsonPropertyName("counterpartyIdentificationStatus")]
		public CounterpartyIdentificationStatus CounterpartyIdentificationStatus { get; set; }
	}
}
