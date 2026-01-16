namespace CustomerAppsApi.Library.Dto.Contacts
{
	/// <summary>
	/// Данные контакта(электронная почта)
	/// </summary>
	public class EmailDto
	{
		/// <summary>
		/// Идентификатор в Erp
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Адрес
		/// </summary>
		public string Address { get; set; }
	}
}
