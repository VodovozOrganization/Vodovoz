namespace CustomerAppsApi.Library.Dto.Counterparties
{
	public class DeleteLegalCounterpartyAccountRequest : GetLegalCustomersDto
	{
		/// <summary>
		/// Идентификатор юр лица
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Электронная почта
		/// </summary>
		public string Email { get; set; }
		//TODO 5633: обговорить что лучше принимать парольво избежание удаления неизвестными лицами или при поломке функционала
		/// <summary>
		/// Пароль
		/// </summary>
		public string Password { get; set; }
	}
}
