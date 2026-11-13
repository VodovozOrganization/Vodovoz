namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Запрос поиска контрагента по адресу
	/// </summary>
	public class CounterpartyAddressSearchDto
	{
		/// <summary>
		/// Город
		/// </summary>
		public string City { get; set; }
		/// <summary>
		/// Улица
		/// </summary>
		public string Street { get; set; }
		/// <summary>
		/// Номер дома
		/// </summary>
		public string Building { get; set; }
		/// <summary>
		/// Корпус
		/// </summary>
		public string Corpus { get; set; }
		/// <summary>
		/// Строение
		/// </summary>
		public string Structure { get; set; }
		/// <summary>
		/// Литера
		/// </summary>
		public string Letter { get; set; }
		/// <summary>
		/// Офис/квартира/помещение
		/// </summary>
		public string Room { get; set; }
		/// <summary>
		/// Помещение
		/// </summary>
		public string Placement { get; set; }
	}
}
