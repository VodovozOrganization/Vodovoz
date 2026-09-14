using System;
using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Данные для обновления сделки по планируемому заказу в Битрикс24 после создания клиентом заказа
	/// </summary>
	public class PlannedOrderDealUpdateDto
	{
		/// <summary>
		/// Id сохранённой в базе данных записи о планируемом заказе
		/// </summary>
		[JsonIgnore]
		public int PlannedOrderId { get; set; }

		/// <summary>
		/// Id обновляемой сделки в Битрикс24
		/// </summary>
		[JsonIgnore]
		public long BitrixDealId { get; set; }

		/// <summary>
		/// Ключ команды обновления сделки в пакетном запросе, содержит id сохранённых данных о планируемом заказе
		/// </summary>
		[JsonIgnore]
		public string DealCommandKey => $"{PlannedOrderDealCommandKeys.UpdateCommandKeyPrefix}{PlannedOrderId}";

		/// <summary>
		/// Стадия сделки в битриксе, в которую переводится сделка после создания заказа клиентом
		/// </summary>
		[JsonPropertyName("STAGE_ID")]
		public string StageId { get; set; }

		/// <summary>
		/// Номер созданного клиентом заказа
		/// </summary>
		[JsonPropertyName("UF_CRM_5EE206A6AD2DB")]
		public int? CreatedOrderId { get; set; }

		/// <summary>
		/// Дата доставки созданного клиентом заказа
		/// </summary>
		[JsonIgnore]
		public DateTime? CreatedOrderDeliveryDate { get; set; }

		/// <summary>
		/// Дата доставки созданного клиентом заказа, заменяет в сделке дату планируемого заказа
		/// </summary>
		[JsonPropertyName("UF_CRM_5ED7643476A24")]
		public string CreatedOrderDeliveryDateString =>
			CreatedOrderDeliveryDate?.ToString("yyyy-MM-dd");
	}
}
