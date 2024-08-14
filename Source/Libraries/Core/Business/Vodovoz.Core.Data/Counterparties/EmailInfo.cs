namespace Vodovoz.Core.Data.Counterparties
{
	/// <summary>
	/// Информация об электронке
	/// </summary>
	public class EmailInfo
	{
		/// <summary>
		/// Id электронной почты в ДВ
		/// </summary>
		public int ErpEmailId { get; set; }
		/// <summary>
		/// Id клиента в ДВ
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Адрес
		/// </summary>
		public string Email { get; set; }
	}
}
