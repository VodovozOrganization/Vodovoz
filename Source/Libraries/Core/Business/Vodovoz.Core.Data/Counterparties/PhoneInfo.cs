namespace Vodovoz.Core.Data.Counterparties
{
	/// <summary>
	/// Информация о телефоне
	/// </summary>
	public class PhoneInfo
	{
		/// <summary>
		/// Id телефона в ДВ
		/// </summary>
		public int ErpPhoneId { get; set; }
		/// <summary>
		/// Id клиента в ДВ
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Номер телефона в формате +7XXXXXXXXXX
		/// </summary>
		public string Number { get; set; }
	}
}
