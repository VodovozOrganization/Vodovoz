namespace TaxcomEdo.Contracts.Organizations
{
	/// <summary>
	/// Информация об организации для ЭДО(электронного документооборота)
	/// </summary>
	public class OrganizationInfoForEdo
	{
		/// <summary>
		/// Id организации
		/// </summary>
		public int Id { get; set; }
		/// <summary>
		/// Название
		/// </summary>
		public string Name { get; set; }
		/// <summary>
		/// Полное название
		/// </summary>
		public string FullName { get; set; }
		/// <summary>
		/// ИНН
		/// </summary>
		public string Inn { get; set; }
		/// <summary>
		/// КПП
		/// </summary>
		public string Kpp { get; set; }
		/// <summary>
		/// Юридический адрес
		/// </summary>
		public string JurAddress { get; set; }
		/// <summary>
		/// Номер кабинета в ЭДО
		/// </summary>
		public string TaxcomEdoAccountId { get; set; }
	}
}
