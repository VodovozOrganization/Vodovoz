namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Данные по запросу эндпойнта получения информации о компании
	/// </summary>
	public class CompanyInfoRequest : GetLegalCustomersDto
	{
		/// <summary>
		/// Идентификатор клиента физического лица
		/// </summary>
		public int ErpCounterpartyId { get; set; }
	}
}
