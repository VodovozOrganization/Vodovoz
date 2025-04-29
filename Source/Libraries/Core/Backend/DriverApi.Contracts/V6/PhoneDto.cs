namespace DriverApi.Contracts.V6
{
	/// <summary>
	/// Телефон
	/// </summary>
	public class PhoneDto
	{
		/// <summary>
		/// Номер телефона
		/// </summary>
		public string Number { get; set; }

		/// <summary>
		/// Тип телефона
		/// </summary>
		public PhoneDtoType PhoneType { get; set; }

		/// <summary>
		/// Информация о телефоне
		/// </summary>
		public string Info { get; set; }
	}
}
