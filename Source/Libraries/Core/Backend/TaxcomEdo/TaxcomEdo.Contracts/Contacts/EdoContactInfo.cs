﻿using System;
using TaxcomEdo.Contracts.Counterparties;

namespace TaxcomEdo.Contracts.Contacts
{
	/// <summary>
	/// Информация о контакте по ЭДО
	/// </summary>
	public class EdoContactInfo
	{
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
		public EdoContactStateCode StateCode { get; set; }
		/// <summary>
		/// Время изменения состояния
		/// </summary>
		public DateTime StateChanged { get; set; }
	}
}
