using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Информация для получения юр лиц по ИНН
	/// </summary>
	public class GetLegalCustomersByInnDto : GetLegalCustomersDto
	{
		/// <summary>
		/// ИНН
		/// </summary>
		public string Inn { get; set; }
		/// <summary>
		/// Id клиента в ERP, делающего запрос
		/// </summary>
		public int ErpCounterpartyId { get; set; }
	}
}
