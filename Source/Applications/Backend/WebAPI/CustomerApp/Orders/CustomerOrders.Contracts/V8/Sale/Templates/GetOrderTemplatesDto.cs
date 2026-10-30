using System;
using CustomerApp.Contracts.Common;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Данные запроса получения яавтозаказов клиента/пользователя
	/// </summary>
	public class GetOrderTemplatesDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public ExternalSource Source { get; set; }

		/// <summary>
		/// Id клиента из ДВ
		/// </summary>
		public int ErpCounterpartyId { get; set; }

		/// <summary>
		/// Идентификатор пользователя
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }

		/// <summary>
		/// Номер страницы
		/// </summary>
		public int Page { get; set; }

		/// <summary>
		/// Количество для отображения на странице
		/// </summary>
		public int TemplatesCountOnPage { get; set; }
	}
}
