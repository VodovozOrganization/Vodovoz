namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Адрес доставки контрагента
	/// </summary>
	public class CounterpartyAddressDto
	{
		/// <summary>
		/// Идентификатор точки доставки в ДВ
		/// </summary>
		public int ErpDeliveryPointId { get; set; }
		/// <summary>
		/// Адрес
		/// </summary>
		public string Address { get; set; }
		/// <summary>
		/// Признак закрытия поставок контрагента (стоп-лист), общий для всех адресов
		/// </summary>
		public bool IsStopList { get; set; }
		/// <summary>
		/// Признак активности точки доставки
		/// </summary>
		public bool IsActive { get; set; }
	}
}
