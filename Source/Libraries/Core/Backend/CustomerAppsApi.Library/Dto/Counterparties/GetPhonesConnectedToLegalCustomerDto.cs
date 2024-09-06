namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Информация для получения телефонов, прикрепленных к юридическому лицу
	/// </summary>
	public class GetPhonesConnectedToLegalCustomerDto : GetLegalCustomersDto
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
