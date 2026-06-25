using System;

namespace CustomerAppsApi.Library.Dto.Counterparties
{
	/// <summary>
	/// Зарегистрированный пользователь (физ лицо)
	/// </summary>
	public class RegisteredNaturalCounterpartyDto
	{
		/// <summary>
		/// Внешний номер пользователя
		/// </summary>
		public Guid ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Id клиента в ДВ
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Имя
		/// </summary>
		public string FirstName { get; set; }
		/// <summary>
		/// Фамилия
		/// </summary>
		public string Surname { get; set; }
		/// <summary>
		/// Отчество
		/// </summary>
		public string Patronymic { get; set; }
		/// <summary>
		/// Электронка
		/// </summary>
		public string Email { get; set; }
		/// <summary>
		/// Телефон
		/// </summary>
		public string PhoneNumber { get; set; }
	}
}
