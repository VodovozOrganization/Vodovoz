using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.Dto.Orders
{
	/// <summary>
	/// Запрос координат курьера
	/// </summary>
	public class CourierCoordinatesRequest
	{
		/// <summary>
		/// Источник запроса <see cref="Vodovoz.Core.Domain.Clients.Source"/>
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Номер заказа из ИПЗ
		/// </summary>
		public Guid ExternalOrderId { get; set; }
		/// <summary>
		/// Подпись отправителя
		/// </summary>
		public string Signature { get; set; }
	}
}
