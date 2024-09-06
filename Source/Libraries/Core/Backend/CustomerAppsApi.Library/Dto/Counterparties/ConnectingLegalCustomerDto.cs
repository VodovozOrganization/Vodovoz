using Vodovoz.Core.Domain.Clients;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Информация для связывания физика и юрика, для возможности заказов этим клиентом от юр лица
	/// </summary>
	public class ConnectingLegalCustomerDto : GetLegalCustomersDto
	{
		/// <summary>
		/// Id юридического лица
		/// </summary>
		public int ErpLegalCounterpartyId { get; set; }
		/// <summary>
		/// Id физика
		/// </summary>
		public int ErpNaturalCounterpartyId { get; set; }
	}
}
