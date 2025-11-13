namespace CustomerAppsApi.Library.Dto.Counterparties
{
	public class LinkingLegalCounterpartyEmailToExternalUser : GetLegalCustomersDto
	{
		/// <summary>
		/// Идентификатор юр лица
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Электронная почта
		/// </summary>
		public string Email { get; set; }
		/// <summary>
		/// Пароль
		/// </summary>
		public string Password { get; set; }
	}
}
