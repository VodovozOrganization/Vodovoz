using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// История заказов контрагента
	/// </summary>
	public class CounterpartyOrderHistoryDto
	{
		/// <summary>
		/// Статус получения истории заказов
		/// </summary>
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public CounterpartySearchStatus Status { get; set; }
		/// <summary>
		/// Идентификатор клиента в ДВ
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Заказы клиента
		/// </summary>
		public IReadOnlyList<CounterpartyOrderDto> Orders { get; set; } = new List<CounterpartyOrderDto>();
		/// <summary>
		/// Описание ошибки, если есть
		/// </summary>
		public string ErrorDescription { get; set; }
	}
}
