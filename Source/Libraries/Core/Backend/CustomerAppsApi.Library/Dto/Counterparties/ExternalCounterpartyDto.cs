using System;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	public abstract class ExternalCounterpartyDto
	{
		/// <summary>
		/// Внешний номер пользователя
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }
	}
}
