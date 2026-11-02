using System;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V4.Dto.Orders
{
	/// <summary>
	/// Информация для проверки применимости промокода
	/// </summary>
	public class PromoCodeWarningDto
	{
		/// <summary>
		/// Источник заказа
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Номер онлайн заказа из ИПЗ
		/// </summary>
		public Guid? ExternalOrderId { get; set; }
		/// <summary>
		/// Подпись, для идентификации источника
		/// </summary>
		public string Signature { get; set; }
		/// <summary>
		/// Промокод
		/// </summary>
		public string PromoCode { get; set; }
	}
}
