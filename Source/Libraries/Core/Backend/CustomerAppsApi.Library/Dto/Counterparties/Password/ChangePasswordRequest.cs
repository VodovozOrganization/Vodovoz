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
		/// <summary>
		/// Старый пароль
		/// </summary>
		public string OldPassword { get; set; }
		/// <summary>
		/// Новый пароль
		/// </summary>
		public string NewPassword { get; set; }
		/// <summary>
		/// Введенный код авторизации
		/// </summary>
		public string AuthorizationCode { get; set; }
	}
}
