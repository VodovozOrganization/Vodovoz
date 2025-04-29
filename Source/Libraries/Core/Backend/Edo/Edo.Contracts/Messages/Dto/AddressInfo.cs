namespace Edo.Contracts.Messages.Dto
{
	/// <summary>
	/// Данные адреса
	/// </summary>
	public class AddressInfo
	{
		/// <summary>
		/// Код страны, откуда адрес(643 - Россия)
		/// </summary>
		public string CountryCode { get; set; } = "643";
		/// <summary>
		/// Наименование страны откуда адрес
		/// </summary>
		public string CountryName { get; set; } = "Россия";
		/// <summary>
		/// Адрес
		/// </summary>
		public string Address { get; set; }
	}
}
