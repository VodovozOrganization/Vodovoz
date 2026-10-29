using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Статус поиска контрагента
	/// </summary>
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum CounterpartySearchStatus : short
	{
		/// <summary>
		/// Ошибка поиска
		/// </summary>
		Error = -1,
		/// <summary>
		/// Контрагент не найден
		/// </summary>
		NotFound = 0,
		/// <summary>
		/// Найден один контрагент
		/// </summary>
		Found = 1,
		/// <summary>
		/// Найдено несколько контрагентов
		/// </summary>
		MultipleFound = 2
	}
}
