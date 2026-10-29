using System;

namespace CustomerAppsApi.Library.V2.Dto.Counterparties
{
	/// <summary>
	/// Информация о контакте пользователя
	/// </summary>
	public class CounterpartyContactInfoDto
	{
		/// <summary>
		/// Номер телефона
		/// </summary>
		public string PhoneNumber { get; set; }
		/// <summary>
		/// Внешний id пользователя из ИПЗ
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Идентификатор откуда клиент
		/// </summary>
		public int CameFromId { get; set; }
	}
}
