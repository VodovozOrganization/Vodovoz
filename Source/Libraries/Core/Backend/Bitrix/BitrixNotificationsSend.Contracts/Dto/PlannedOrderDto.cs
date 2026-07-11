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
		/// Наименование контрагента
		/// </summary>
		[JsonPropertyName("counterpartyName")]
		public string CounterpartyName { get; set; }

		/// <summary>
		/// ИНН контрагента
		/// </summary>
		[JsonPropertyName("counterpartyInn")]
		public string CounterpartyInn { get; set; }

		/// <summary>
		/// Номер для связи, указанный в последнем выполненном заказе
		/// </summary>
		[JsonPropertyName("phoneNumber")]
		public string PhoneNumber { get; set; }

		/// <summary>
		/// Адрес электронной почты
		/// </summary>
		[JsonPropertyName("emailAdress")]
		public string EmailAddress { get; set; }

		/// <summary>
		/// Адрес точки доставки, пусто - для самовывоза
		/// </summary>
		[JsonPropertyName("deliveryPointAddress")]
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
		[JsonPropertyName("selfDelivery")]
		public string SelfDelivery =>
			IsSelfDelivery
			? "Да"
			: "Нет";

		/// <summary>
		/// Дата доставки последнего выполненного заказа
		/// </summary>
		[JsonPropertyName("lastOrderDeliveryDate")]
		public DateTime LastOrderDeliveryDate { get; set; }

		/// <summary>
		/// Дата планируемого заказа
		/// </summary>
		[JsonPropertyName("plannedOrderDate")]
		public DateTime PlannedOrderDate { get; set; }

		/// <summary>
		/// Количество бутылей в последнем выполненном заказе
		/// </summary>
		[JsonPropertyName("lastOrderBottlesCount")]
		public int LastOrderBottlesCount { get; set; }

		/// <summary>
		/// Долг по бутылям по адресу, null - для самовывоза
		/// </summary>
		[JsonPropertyName("bottleDebtByAddress")]
		public int? BottlesDebtByAddress { get; set; }

		/// <summary>
		/// Долг по бутылям по клиенту
		/// </summary>
		[JsonPropertyName("bottleDebt")]
		public int BottlesDebtByCounterparty { get; set; }

		/// <summary>
		/// Отсрочка по оплате для контрагента в днях, 0 - для физических лиц
		/// </summary>
		[JsonPropertyName("daysOfDeferment")]
		public int DelayDaysForCounterparty { get; set; }

		/// <summary>
		/// Общая дебиторская задолженность, 0 - для физических лиц
		/// </summary>
		[JsonPropertyName("accountsReceivable")]
		public decimal DebtorDebt { get; set; }

		/// <summary>
		/// Дата и время выгрузки данных
		/// </summary>
		[JsonPropertyName("unloadingDate")]
		public DateTime UnloadingDate =>
			DateTime.Today;
	}
}
