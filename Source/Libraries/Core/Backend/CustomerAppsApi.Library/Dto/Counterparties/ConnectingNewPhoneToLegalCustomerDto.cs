namespace CustomerAppsApi.Library.Dto.Counterparties
{
	public class ConnectingNewPhoneToLegalCustomerDto : GetLegalCustomersDto
	{
		/// <summary>
		/// Id юридического лица
		/// </summary>
		public int ErpLegalCounterpartyId { get; set; }
		/// <summary>
		/// Id физика, от которого запрос
		/// </summary>
		public int ErpNaturalCounterpartyId { get; set; }
		/// <summary>
		/// Номер телефона для связки в формате ХХХХХХХХХХ
		/// </summary>
		public string PhoneNumber { get; set; }
	}
}
