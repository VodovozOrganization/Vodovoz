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
		public string DealCommandKey => $"{ChainStoreDelayDealCommandKeys.CreateCommandKeyPrefix}{ChainStoreDelayBitrixDealId}";

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
		/// Адрес доставки заказа.
		/// </summary>
		[JsonPropertyName("UF_CRM_5DA85CF9E13B9")]
		public string DeliveryAddress { get; set; }

		/// <summary>
		/// Номер заказа.
		/// </summary>
		[JsonPropertyName("UF_CRM_1590590364997")]
		public int OrderId { get; set; }

		/// <summary>
		/// Автор заказа.
		/// </summary>
		[JsonPropertyName("UF_CRM_1618412191621")]
		public string OrderAuthor { get; set; }

		/// <summary>
		/// Сумма заказа. Валюта (CURRENCY_ID) не передаётся.
		/// </summary>
		[JsonPropertyName("OPPORTUNITY")]
		public decimal OrderSum { get; set; }

		/// <summary>
		/// Расчётная дата платежа заказа.
		/// </summary>
		[JsonIgnore]
		public DateTime EstimatedPaymentDate { get; set; }

		/// <summary>
		/// Расчётная дата платежа заказа для Битрикс24.
		/// </summary>
		[JsonPropertyName("UF_CRM_1721989000906")]
		public string EstimatedPaymentDateString =>
			EstimatedPaymentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

		/// <summary>
		/// Дни просрочки (всегда 0).
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
		/// Долг по заказу.
		/// </summary>
		[JsonIgnore]
		public decimal Debt { get; set; }

		/// <summary>
		/// Долг по заказу для Битрикс24.
		/// </summary>
		[JsonPropertyName("UF_CRM_1760695840735")]
		public string DebtString =>
			Debt.ToString("0.00", CultureInfo.InvariantCulture);
	}
}
