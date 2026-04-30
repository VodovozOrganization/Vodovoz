using System;

namespace CustomerOrders.Contracts.V4.Orders
{
	/// <summary>
	/// Информация для проверки применимости промокода
	/// </summary>
	public class PromoCodeWarningDto
	{
		/// <summary>
		/// Источник заказа
		/// </summary>
		public ExternalSource Source { get; set; }
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
