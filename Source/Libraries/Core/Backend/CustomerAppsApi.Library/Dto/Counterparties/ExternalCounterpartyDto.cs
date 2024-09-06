using System;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Информация о id внешнего пользователя
	/// </summary>
	public abstract class ExternalCounterpartyDto
	{
		/// <summary>
		/// Внешний номер пользователя
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }
	}
}
