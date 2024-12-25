namespace Edo.Contracts.Messages.Dto
{
	/// <summary>
	/// Информация об организации
	/// </summary>
	public class OrganizationInfo
	{
		/// <summary>
		/// Наименование
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Адрес
		/// </summary>
		public AddressInfo Address { get; set; }
		/// <summary>
		/// ИНН
		/// </summary>
		public string Inn { get; set; }
		/// <summary>
		/// КПП
		/// </summary>
		public string Kpp { get; set; }
		/// <summary>
		/// Номер кабинета в ЭДО
		/// </summary>
		public string EdoAccountId { get; set; }
	}
}
