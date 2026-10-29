using System;
using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Данные недовоза для создания сделки в Битрикс24.
	/// </summary>
	public class UndeliveredOrderDto
	{
		/// <summary>
		/// Id воронки "ЛО: Спасение недовозов".
		/// </summary>
		[JsonPropertyName("CATEGORY_ID")]
		public int CategoryId => 325;

		/// <summary>
		/// Начальная стадия сделки в воронке.
		/// </summary>
		[JsonPropertyName("STAGE_ID")]
		public string StageId => "C325:NEW";

		/// <summary>
		/// Название сделки.
		/// </summary>
		[JsonPropertyName("TITLE")]
		public string Title { get; set; }

		/// <summary>
		/// Идентификатор локальной записи синхронизации недовоза.
		/// </summary>
		[JsonIgnore]
		public int UndeliveredOrderBitrixDealId { get; set; }

		/// <summary>
		/// Идентификатор сделки в Битрикс24.
		/// </summary>
		[JsonIgnore]
		public long? BitrixDealId { get; set; }

		/// <summary>
		/// Ключ команды создания сделки в пакетном запросе.
		/// </summary>
		[JsonIgnore]
		public string DealCommandKey => $"undelivered_order_deal_{UndeliveredOrderBitrixDealId}";

		/// <summary>
		/// Номер заказа.
		/// </summary>
		[JsonIgnore]
		public string OrderId { get; set; }

		/// <summary>
		/// Номер заказа для Битрикс24.
		/// </summary>
		[JsonPropertyName("UF_CRM_1590590364997")]
		public int? OrderIdValue =>
			int.TryParse(OrderId, out var orderId)
				? (int?)orderId
				: null;

		/// <summary>
		/// Наименование клиента.
		/// </summary>
		[JsonPropertyName("UF_CRM_1662373256")]
		public string CounterpartyName { get; set; }

		/// <summary>
		/// Адрес доставки.
		/// </summary>
		[JsonPropertyName("UF_CRM_5E006F6809EDF")]
		public string DeliveryAddress { get; set; }

		/// <summary>
		/// Дата заказа.
		/// </summary>
		[JsonIgnore]
		public DateTime? DeliveryDate { get; set; }

		/// <summary>
		/// Дата заказа для Битрикс24.
		/// </summary>
		[JsonPropertyName("UF_CRM_5ED7643476A24")]
		public string DeliveryDateString =>
			DeliveryDate?.ToString("yyyy-MM-dd");

		/// <summary>
		/// Интервал доставки.
		/// </summary>
		[JsonPropertyName("UF_CRM_1618414046622")]
		public string DeliveryInterval { get; set; }

		/// <summary>
		/// ФИО водителя.
		/// </summary>
		[JsonPropertyName("UF_CRM_1666873517")]
		public string DriverName { get; set; }

		/// <summary>
		/// Маршрутный лист.
		/// </summary>
		[JsonPropertyName("UF_CRM_1652267372594")]
		public string RouteListId { get; set; }

		/// <summary>
		/// Вид недовоза.
		/// </summary>
		[JsonPropertyName("UF_CRM_1781877313207")]
		public string TypeOfUndelivery { get; set; }

		/// <summary>
		/// Детализация недовоза.
		/// </summary>
		[JsonPropertyName("UF_CRM_1781877804915")]
		public string SpecificationOfUndelivery { get; set; }

		/// <summary>
		/// Комментарий о причине недовоза.
		/// </summary>
		[JsonPropertyName("UF_CRM_5F524065B5DF9")]
		public string Comment { get; set; }

		/// <summary>
		/// Номер нового заказа.
		/// </summary>
		[JsonIgnore]
		public string NewOrderId { get; set; }

		/// <summary>
		/// Номер нового заказа для Битрикс24.
		/// </summary>
		[JsonPropertyName("UF_CRM_5EE206A6AD2DB")]
		public int? NewOrderIdValue =>
			int.TryParse(NewOrderId, out var newOrderId)
				? (int?)newOrderId
				: null;
	}
}
