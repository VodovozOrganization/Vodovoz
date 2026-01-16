using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Информация об источнике запроса (ИПЗ)
	/// </summary>
	public abstract class GetLegalCustomersDto : ExternalCounterpartyDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }
	}
}
