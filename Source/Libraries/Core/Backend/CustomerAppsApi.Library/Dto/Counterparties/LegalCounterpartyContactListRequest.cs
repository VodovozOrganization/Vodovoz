namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Данные для получения контактов юр лица
	/// </summary>
	public class LegalCounterpartyContactListRequest : GetLegalCustomersDto
	{
		/// <summary>
		/// Идентификатор юр лица
		/// </summary>
		public int ErpCounterpartyId { get; set; }
	}
}
