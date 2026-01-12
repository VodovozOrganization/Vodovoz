using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto.Edo
{
	public interface IFindExternalLegalCounterpartyAccountDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; }
		/// <summary>
		/// Идентификатор пользователя
		/// </summary>
		public Guid ExternalCounterpartyId { get; }
		/// <summary>
		/// Идентификатор клиента в Erp
		/// </summary>
		public int CounterpartyErpId { get; }
	}
}
