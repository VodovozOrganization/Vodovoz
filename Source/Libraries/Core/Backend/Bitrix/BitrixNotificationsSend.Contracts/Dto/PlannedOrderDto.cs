using System;
using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Данные о точке доставки (или самовывозе) клиента, по которой ожидается плановый заказ
	/// </summary>
	public class PlannedOrderDto
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
		public int CategoryId => 321;

		/// <summary>
		/// Стадия сделки в битриксе
		/// </summary>
		[JsonPropertyName("STAGE_ID")]
		public string StageId => "NEW";

		/// <summary>
		/// Id контрагента
		/// </summary>
		[JsonIgnore]
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Id точки доставки, null - для самовывоза
		/// </summary>
		[JsonIgnore]
		public int? DeliveryPointId { get; set; }

		/// <summary>
		/// Ключ команды создания сделки в пакетном запросе,
		/// содержит id контрагента и точки доставки (self - для самовывоза)
		/// </summary>
		[JsonIgnore]
		public string DealCommandKey =>
			DeliveryPointId.HasValue
			? $"deal_{CounterpartyId}_{DeliveryPointId.Value}"
			: $"deal_{CounterpartyId}_self";

		/// <summary>
		/// Наименование контрагента
		/// </summary>
		[JsonPropertyName("UF_CRM_1662373256")]
		public string CounterpartyName { get; set; }

		/// <summary>
		/// ИНН контрагента
		/// </summary>
		[JsonPropertyName("UF_CRM_1729861318237")]
		public string CounterpartyInn { get; set; }

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
		/// Адрес точки доставки, пусто - для самовывоза
		/// </summary>
		[JsonPropertyName("UF_CRM_5DA85CF9E13B9")]
		public string DeliveryPointAddress { get; set; }

		/// <summary>
		/// Признак самовывоза
		/// </summary>
		[JsonIgnore]
		public bool IsSelfDelivery { get; set; }

		/// <summary>
		/// Самовывоз
		/// "Да" - если самовывоз
		/// "Нет" - если доставка на точку доставки
		/// </summary>
		[JsonPropertyName("UF_CRM_1777379840190")]
		public int SelfDelivery =>
			IsSelfDelivery
			? 1
			: 0;

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

		/// <summary>
		/// Дата планируемого заказа
		/// </summary>
		[JsonIgnore]
		public DateTime PlannedOrderDate { get; set; }

		/// <summary>
		/// Дата планируемого заказа
		/// </summary>
		[JsonPropertyName("UF_CRM_5ED7643476A24")]
		public string PlannedOrderString =>
			PlannedOrderDate.ToString("yyyy-MM-dd");

		/// <summary>
		/// Количество бутылей в последнем выполненном заказе
		/// </summary>
		[JsonPropertyName("UF_CRM_1661426633939")]
		public int LastOrderBottlesCount { get; set; }

		/// <summary>
		/// Долг по бутылям по адресу, null - для самовывоза
		/// </summary>
		[JsonIgnore]
		public int? BottlesDebtByAddress { get; set; }

		/// <summary>
		/// Долг по бутылям по адресу, null - для самовывоза
		/// </summary>
		[JsonPropertyName("UF_CRM_1696873817133")]
		public string BottlesDebtByAddressString =>
			BottlesDebtByAddress.HasValue
			? BottlesDebtByAddress.Value.ToString()
			: string.Empty;

		/// <summary>
		/// Долг по бутылям по клиенту
		/// </summary>
		[JsonIgnore]
		public int BottlesDebtByCounterparty { get; set; }

		/// <summary>
		/// Долг по бутылям по клиенту
		/// </summary>
		[JsonPropertyName("UF_CRM_1777380183821")]
		public string BottlesDebtByCounterpartyString =>
			BottlesDebtByCounterparty.ToString();

		/// <summary>
		/// Отсрочка по оплате для контрагента в днях, 0 - для физических лиц
		/// </summary>
		[JsonIgnore]
		public int DelayDaysForCounterparty { get; set; }

		/// <summary>
		/// Отсрочка по оплате для контрагента в днях, 0 - для физических лиц
		/// </summary>
		[JsonPropertyName("UF_CRM_1759475455713")]
		public string DelayDaysForCounterpartyString =>
			DelayDaysForCounterparty.ToString();

		/// <summary>
		/// Общая дебиторская задолженность, 0 - для физических лиц
		/// </summary>
		[JsonIgnore]
		public decimal DebtorDebt { get; set; }

		/// <summary>
		/// Общая дебиторская задолженность, 0 - для физических лиц
		/// </summary>
		[JsonPropertyName("UF_CRM_1759474068097")]
		public string DebtorDebtString =>
			DebtorDebt.ToString();
	}
}
