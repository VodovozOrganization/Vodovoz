using System;
using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Данные о последнем сервисном заказе клиента, по которому ожидается создание сделки
	/// </summary>
	public class LastServiceOrderDto
	{
		/// <summary>
		/// Наименование сделки
		/// </summary>
		[JsonPropertyName("TITLE")]
		public string Title => CounterpartyName;

		/// <summary>
		/// Id вороки в битриксе, в которую будет помещена сделка
		/// </summary>
		[JsonPropertyName("CATEGORY_ID")]
		public int CategoryId => 165;

		/// <summary>
		/// Стадия сделки в битриксе
		/// </summary>
		[JsonPropertyName("STAGE_ID")]
		public string StageId => "NEW";

		/// <summary>
		/// Ключ команды создания сделки в пакетном запросе,
		/// содержит id сохранённых данных о последнем сервисном заказе
		/// </summary>
		[JsonIgnore]
		public string DealCommandKey => $"deal_{LastServiceOrderId}";

		/// <summary>
		/// Наименование контрагента
		/// </summary>
		[JsonIgnore]
		public string CounterpartyName { get; set; }

		/// <summary>
		/// Id сохранённой в базе данных записи о последнем сервисном заказе
		/// </summary>
		[JsonIgnore]
		public int LastServiceOrderId { get; set; }

		/// <summary>
		/// Id контрагента
		/// </summary>
		[JsonIgnore]
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Id контрагента
		/// </summary>
		[JsonPropertyName("UF_CRM_1589369986950")]
		public string CounterpartyIdString =>
			CounterpartyId.ToString();

		/// <summary>
		/// Адрес точки доставки, пусто - для самовывоза
		/// </summary>
		[JsonPropertyName("UF_CRM_5DA85CF9E13B9")]
		public string DeliveryPointAddress { get; set; }

		/// <summary>
		/// Номер для связи, указанный в последнем выполненном заказе
		/// </summary>
		[JsonPropertyName("UF_CRM_1580981695580")]
		public string PhoneNumber { get; set; }

		/// <summary>
		/// Адрес электронной почты
		/// </summary>
		[JsonPropertyName("UF_CRM_1658917933")]
		public string EmailAddress { get; set; }

		/// <summary>
		/// Дата доставки последнего выполненного заказа
		/// </summary>
		[JsonIgnore]
		public DateTime LastOrderDeliveryDate { get; set; }

		/// <summary>
		/// Дата доставки последнего выполненного заказа
		/// </summary>
		[JsonPropertyName("UF_CRM_5E006F67E6DAC")]
		public string LastOrderDeliveryDateString =>
			LastOrderDeliveryDate.ToString("yyyy-MM-dd");
	}
}
