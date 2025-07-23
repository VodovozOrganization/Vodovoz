using System;

namespace SecureCodeSender.Contracts.Requests
{
	public class CheckSecureCodeDto
	{
		/// <summary>
		/// Код авторизации
		/// </summary>
		public string Code { get; set; }
		/// <summary>
		/// Куда отправлялся код
		/// </summary>
		public string Target { get; set; }
		/// <summary>
		/// Телефон пользователя в формате 7XXXXXXXXXX
		/// </summary>
		public string UserPhone { get; set; }
		/// <summary>
		/// Источник запроса
		/// </summary>
		public int Source { get; set; }
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
		public Guid ExternalCounterpartyId { get; set; }
	}
}
