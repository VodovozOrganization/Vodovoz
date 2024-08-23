﻿using System.Collections.Generic;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Data.Counterparties
{
	/// <summary>
	/// Информация о юр лице
	/// </summary>
	public class LegalCounterpartyInfo
	{
		/// <summary>
		/// Id клиента в ДВ
		/// </summary>
		public int ErpCounterpartyId { get; set; }
		/// <summary>
		/// Полное наименование
		/// </summary>
		public string FullName { get; set; }
		/// <summary>
		/// ИНН
		/// </summary>
		public string Inn { get; set; }
		/// <summary>
		/// КПП
		/// </summary>
		public string Kpp { get; set; }
		/// <summary>
		/// Юр адрес
		/// </summary>
		public string JurAddress { get; set; }
		/// <summary>
		/// Состояние связи <see cref="ConnectedCustomerConnectState"/>
		/// </summary>
		public string ConnectState { get; set; }
		/// <summary>
		/// Список телефонов
		/// </summary>
		public IEnumerable<PhoneInfo> Phones { get; set; }
		/// <summary>
		/// Список эл адресов
		/// </summary>
		public IEnumerable<EmailInfo> Emails { get; set; }
	}
}
