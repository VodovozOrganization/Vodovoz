namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Данные по регистрации клиента
	/// </summary>
	public class CounterpartyRegistrationDto
	{
		/// <summary>
		/// Идентификатор клиента из ДВ
		/// </summary>
		public int? ErpCounterpartyId { get; set; }
		/// <summary>
		/// Описание ошибки, если есть
		/// </summary>
		public string ErrorDescription { get; set; }
		/// <summary>
		/// Статус регистрации <see cref="CustomerAppsApi.Library.V2.Dto.Counterparties.CounterpartyRegistrationStatus"/>
		/// </summary>
		public CounterpartyRegistrationStatus CounterpartyRegistrationStatus { get; set; }
	}
}
