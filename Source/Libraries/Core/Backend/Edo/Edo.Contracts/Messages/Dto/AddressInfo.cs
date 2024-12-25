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
		/// Адрес
		/// </summary>
		public string Address { get; set; }
	}
}
