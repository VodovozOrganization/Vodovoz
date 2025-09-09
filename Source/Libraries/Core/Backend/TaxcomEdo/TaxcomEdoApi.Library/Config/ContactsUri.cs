namespace TaxcomEdoApi.Library.Config
{
	/// <summary>
	/// Эндпойнты для работы со списком контактов
	/// </summary>
	public sealed class ContactsUri
	{
		/// <summary>
		/// Эндпойнт принятия контакта
		/// </summary>
		public string AcceptContactUri { get; set; }
		/// <summary>
		/// Эндпойнт отбраковки контакта
		/// </summary>
		public string RejectContactUri { get; set; }
		/// <summary>
		/// Эндпойнт отправки приглашений
		/// </summary>
		public string SendContactsUri { get; set; }
		/// <summary>
		/// Эндпойнт проверки клиента на сервере Такском
		/// </summary>
		public string CheckContragentUri { get; set; }
	}
}
