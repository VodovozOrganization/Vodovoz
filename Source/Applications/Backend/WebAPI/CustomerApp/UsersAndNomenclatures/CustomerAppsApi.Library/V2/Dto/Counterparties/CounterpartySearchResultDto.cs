using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Результат поиска контрагента
	/// </summary>
	public class CounterpartySearchResultDto
	{
		/// <summary>
		/// Статус поиска
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public CounterpartySearchStatus Status { get; set; }
		/// <summary>
		/// Найденные контрагенты
		/// </summary>
		public IReadOnlyList<CounterpartyShortDto> Counterparties { get; set; } = new List<CounterpartyShortDto>();
		/// <summary>
		/// Описание ошибки, если есть
		/// </summary>
		public string ErrorDescription { get; set; }
	}
}
