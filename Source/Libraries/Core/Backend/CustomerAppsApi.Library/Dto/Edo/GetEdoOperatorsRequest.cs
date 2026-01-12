using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto.Edo
{
	/// <summary>
	/// Данные запроса получения операторов ЭДО
	/// </summary>
	public class GetEdoOperatorsRequest
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Идентификатор пользователя
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Идентификатор клиента в Erp
		/// </summary>
		public int CounterpartyErpId { get; set; }
	}
}
