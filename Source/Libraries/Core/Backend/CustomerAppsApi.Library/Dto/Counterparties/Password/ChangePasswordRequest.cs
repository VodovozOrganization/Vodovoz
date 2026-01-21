namespace CustomerAppsApi.Library.Dto.Counterparties.Password
{
	/// <summary>
	/// Данные для запроса смены пароля
	/// </summary>
	public class ChangePasswordRequest : GetLegalCustomersDto
	{
		/// <summary>
		/// Идентификатор юр лица
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Электронная почта
		/// </summary>
		public string Email { get; set; }
		//TODO 5633: обговорить что лучше принимать и старый пароль и новый во избежание смены неизвестными лицами или при поломке функционала
		/// <summary>
		/// Старый пароль
		/// </summary>
		public string OldPassword { get; set; }
		/// <summary>
		/// Новый пароль
		/// </summary>
		public string NewPassword { get; set; }
	}
}
