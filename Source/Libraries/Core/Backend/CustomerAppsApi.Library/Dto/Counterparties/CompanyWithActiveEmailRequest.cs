namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Данные запроса для получения идентификаторов юр лиц с активной почтой
	/// </summary>
	public class CompanyWithActiveEmailRequest : GetLegalCustomersDto
	{
		/// <summary>
		/// Электронная почта
		/// </summary>
		public string Email { get; set; }
	}
}
