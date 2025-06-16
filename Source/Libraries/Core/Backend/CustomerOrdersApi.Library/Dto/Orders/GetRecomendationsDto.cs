using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Library.Dto.Orders
{
	/// <summary>
	/// Запрос на получение рекомендаций к заказу
	/// </summary>
	public class GetRecomendationsDto
	{
		/// <summary>
		/// Источник запроса
		/// </summary>
		public Source Source { get; set; }
		/// <summary>
		/// Контрольная сумма, для проверки валидности отправителя
		/// </summary>
		public string Signature { get; set; }

		/// <summary>
		/// Номер онлайн заказа из ИПЗ
		/// </summary>
		public Guid? ExternalOrderId { get; set; }

		/// <summary>
		/// Id клиента в ИПЗ
		/// </summary>
		public Guid? ExternalCounterpartyId { get; set; }

		/// <summary>
		/// Id контрагента в ДВ
		/// </summary>
		public int? ErpCounterpartyId { get; set; }

		/// <summary>
		/// Id точки доставки в ДВ
		/// </summary>
		public int? ErpDeliveryPointId { get; set; }

		/// <summary>
		/// Идентификаторы товаров в корзине
		/// </summary>
		public IEnumerable<int> AddedNomenclatureIds { get; set; }
	}
}
