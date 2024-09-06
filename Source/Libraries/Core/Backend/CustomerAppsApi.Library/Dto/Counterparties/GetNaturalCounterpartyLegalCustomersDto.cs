namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Информация о id клиента в ERP, для получения привязанных к нему юр лиц
	/// </summary>
	public class GetNaturalCounterpartyLegalCustomersDto : GetLegalCustomersDto
	{
		/// <summary>
		/// Id клиента в ERP, делающего запрос
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Номер телефона для связки в формате ХХХХХХХХХХ
		/// </summary>
		public string PhoneNumber { get; set; }
	}
}
