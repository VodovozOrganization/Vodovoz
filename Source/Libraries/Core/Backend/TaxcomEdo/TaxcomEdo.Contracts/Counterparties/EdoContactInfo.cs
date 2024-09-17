namespace TaxcomEdo.Contracts.Counterparties
{
	/// <summary>
	/// Информация о контакте по ЭДО
	/// </summary>
	public class EdoContactInfo
	{
		public static readonly string ExchangeAndQueueName = "contacts-info";
		
		/// <summary>
		/// Номер кабинета ЭДО
		/// </summary>
		public string EdxClientId { get; set; }
		/// <summary>
		/// ИНН клиента
		/// </summary>
		public string Inn { get; set; }
		/// <summary>
		/// Статус
		/// </summary>
		public string StateCode { get; set; }
	}
}
