namespace TaxcomEdo.Contracts.Counterparties
{
	/// <summary>
	/// Информация о контатке по ЭДО
	/// </summary>
	public class EdoContactInfo
	{
		protected EdoContactInfo(string edxClientId, string inn, string stateCode)
		{
			EdxClientId = edxClientId;
			Inn = inn;
			StateCode = stateCode;
		}
		
		/// <summary>
		/// Номер кабинета ЭДО
		/// </summary>
		public string EdxClientId { get; }
		/// <summary>
		/// ИНН клиента
		/// </summary>
		public string Inn { get; }
		/// <summary>
		/// Статус
		/// </summary>
		public string StateCode { get; }

		public static EdoContactInfo Create(string edxClientId, string inn, string stateCode)
		{
			return new EdoContactInfo(edxClientId, inn, stateCode);
		}
	}
}
