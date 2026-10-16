using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Данные отправки сделки по отсрочке сети для создания сделки в Битрикс24.
	/// </summary>
	public class ChainStoreDelayDealDto
	{
		/// <summary>
		/// Id воронки. Задаётся из настроек.
		/// </summary>
		[JsonPropertyName("CATEGORY_ID")]
		public int CategoryId { get; set; }

		/// <summary>
		/// Начальная стадия сделки в воронке. Задаётся из настроек.
		/// </summary>
		[JsonPropertyName("STAGE_ID")]
		public string StageId { get; set; }

		/// <summary>
		/// Название сделки.
		/// </summary>
		[JsonPropertyName("TITLE")]
		public string Title { get; set; }

		/// <summary>
		/// Идентификатор локальной записи отправки сделки по отсрочке сети.
		/// </summary>
		[JsonIgnore]
		public int ChainStoreDelayBitrixDealId { get; set; }

		/// <summary>
		/// Ключ команды создания сделки в пакетном запросе.
		/// </summary>
		[JsonIgnore]
		public string DealCommandKey => $"chain_store_delay_deal_{ChainStoreDelayBitrixDealId}";

		/// <summary>
		/// Наименование клиента.
		/// </summary>
		[JsonPropertyName("UF_CRM_1662373256")]
		public string CounterpartyName { get; set; }

		/// <summary>
		/// Телефон для связи. Если не задан, поле не передаётся.
		/// </summary>
		[JsonPropertyName("UF_CRM_1580981695580")]
		public string PhoneNumber { get; set; }

		/// <summary>
		/// ИНН клиента.
		/// </summary>
		[JsonPropertyName("UF_CRM_1729861318237")]
		public string CounterpartyInn { get; set; }

		/// <summary>
		/// Адреса доставки по группе заказов.
		/// </summary>
		[JsonPropertyName("UF_CRM_5DA85CF9E13B9")]
		public string DeliveryAddresses { get; set; }

		/// <summary>
		/// Номер заказа-инициатора.
		/// </summary>
		[JsonPropertyName("UF_CRM_1590590364997")]
		public int InitiatorOrderId { get; set; }

		/// <summary>
		/// Авторы заказов группы.
		/// </summary>
		[JsonPropertyName("UF_CRM_1618412191621")]
		public string OrderAuthors { get; set; }

		/// <summary>
		/// Сумма по группе заказов. Валюта (CURRENCY_ID) не передаётся.
		/// </summary>
		[JsonPropertyName("OPPORTUNITY")]
		public decimal OrdersSum { get; set; }

		/// <summary>
		/// Расчётная дата платежа заказа-инициатора.
		/// </summary>
		[JsonIgnore]
		public DateTime InitiatorEstimatedPaymentDate { get; set; }

		/// <summary>
		/// Расчётная дата платежа заказа-инициатора для Битрикс24.
		/// </summary>
		[JsonPropertyName("UF_CRM_1721989000906")]
		public string InitiatorEstimatedPaymentDateString =>
			InitiatorEstimatedPaymentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

		/// <summary>
		/// Дни просрочки самого раннего заказа группы.
		/// </summary>
		[JsonIgnore]
		public int DaysOverdue { get; set; }

		/// <summary>
		/// Дни просрочки для Битрикс24.
		/// </summary>
		[JsonPropertyName("UF_CRM_1686746335103")]
		public string DaysOverdueString =>
			DaysOverdue.ToString(CultureInfo.InvariantCulture);

		/// <summary>
		/// Долг по группе заказов.
		/// </summary>
		[JsonIgnore]
		public decimal DebtSum { get; set; }

		/// <summary>
		/// Долг по группе заказов для Битрикс24.
		/// </summary>
		[JsonPropertyName("UF_CRM_1760695840735")]
		public string DebtSumString =>
			DebtSum.ToString("0.00", CultureInfo.InvariantCulture);
	}
}
