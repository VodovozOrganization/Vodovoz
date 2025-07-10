using System;
using Contracts.Common;

namespace Contracts.Requests
{
	/// <summary>
	/// Данные по запросу формирования и отправки авторизационного кода
	/// </summary>
	public class SendSecureCodeDto
	{
		/// <summary>
		/// Тип отправки <see cref="Contracts.Common.SendTo"/>
		/// </summary>
		public SendTo Method { get; set; }
		/// <summary>
		/// Куда отправляем код
		/// </summary>
		public string Target { get; set; }
		/// <summary>
		/// Телефон пользователя
		/// </summary>
		public string UserPhone { get; set; }
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Ip адрес пользователя
		/// </summary>
		public string Ip { get; set; }
		/// <summary>
		/// Характеристика клиента
		/// </summary>
		public string UserAgent { get; set; }
		/// <summary>
		/// Id клиента в Erp
		/// </summary>
		public int? ErpCounterpartyId { get; set; }
		/// <summary>
		/// Id клиента/пользователя в ИПЗ
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }
	}
}
