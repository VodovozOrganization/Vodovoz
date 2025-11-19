namespace CustomerAppsApi.Library.Dto.Contacts
{
	/// <summary>
	/// Данные контакта(телефон)
	/// </summary>
	public class PhoneDto
	{
		/// <summary>
		/// Идентификатор в Erp
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Номер телефона
		/// </summary>
		public string Number { get; set; }
	}
}
