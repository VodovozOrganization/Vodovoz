namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Запрос поиска контрагента по телефону
	/// </summary>
	public class CounterpartyPhoneSearchDto
	{
		/// <summary>
		/// Номер телефона в любом формате
		/// </summary>
		public string PhoneNumber { get; set; }
	}
}
