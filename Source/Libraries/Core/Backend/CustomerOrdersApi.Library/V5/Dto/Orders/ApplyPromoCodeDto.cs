using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vodovoz.Core.Data.Orders.V5;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	/// <summary>
	/// Информация для проверки применимости промокода
	/// </summary>
	public class ApplyPromoCodeDto
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
		/// Id контрагента в ДВ
		/// </summary>
		public int? ErpCounterpartyId { get; set; }
		/// <summary>
		/// Контрольная сумма, для проверки валидности отправителя
		/// </summary>
		public string Signature { get; set; }
		/// <summary>
		/// Id клиента в ИПЗ
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }
		/// <summary>
		/// Промокод
		/// </summary>
		public string PromoCode { get; set; }
		/// <summary>
		/// Список товаров
		/// </summary>
		public IEnumerable<OnlineOrderItemDto> OnlineOrderItems { get; set; }
		[JsonIgnore]
		public DateTime RequestTime { get; } = DateTime.UtcNow;
	}
}
