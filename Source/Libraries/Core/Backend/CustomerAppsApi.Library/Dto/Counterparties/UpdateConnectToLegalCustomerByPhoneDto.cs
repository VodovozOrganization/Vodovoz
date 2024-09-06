namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Информация для обновления связи конкретного телефона
	/// </summary>
	public class UpdateConnectToLegalCustomerByPhoneDto : GetLegalCustomersDto
	{
		/// <summary>
		/// Id юридического лица
		/// </summary>
		public int ErpLegalCounterpartyId { get; set; }
		/// <summary>
		/// Id физика
		/// </summary>
		public int ErpNaturalCounterpartyId { get; set; }
		/// <summary>
		/// Id телефона в ERP
		/// </summary>
		public int ErpPhoneId { get; set; }
		/// <summary>
		/// Статус связи номера с юр лицом <see cref="ConnectedCustomerPhoneConnectState"/>
		/// </summary>
		public string ConnectState { get; set; }
	}
}
