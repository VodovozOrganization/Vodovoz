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
		public string CounterpartyName { get; set; }

		/// <summary>
		/// ИНН контрагента
		/// </summary>
		public string CounterpartyInn { get; set; }

		/// <summary>
		/// Номер для связи, указанный в последнем выполненном заказе
		/// </summary>
		public string PhoneNumber { get; set; }

		/// <summary>
		/// Адрес электронной почты
		/// </summary>
		public string EmailAddress { get; set; }

		/// <summary>
		/// Адрес точки доставки, пусто - для самовывоза
		/// </summary>
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
		public string SelfDelivery =>
			IsSelfDelivery
			? "Да"
			: "Нет";

		/// <summary>
		/// Дата доставки последнего выполненного заказа
		/// </summary>
		public DateTime LastOrderDeliveryDate { get; set; }

		/// <summary>
		/// Дата планируемого заказа
		/// </summary>
		public DateTime PlannedOrderDate { get; set; }

		/// <summary>
		/// Количество бутылей в последнем выполненном заказе
		/// </summary>
		public int LastOrderBottlesCount { get; set; }

		/// <summary>
		/// Долг по бутылям по адресу, null - для самовывоза
		/// </summary>
		public int? BottlesDebtByAddress { get; set; }

		/// <summary>
		/// Долг по бутылям по клиенту
		/// </summary>
		public int BottlesDebtByCounterparty { get; set; }

		/// <summary>
		/// Отсрочка по оплате для контрагента в днях, 0 - для физических лиц
		/// </summary>
		public int DelayDaysForCounterparty { get; set; }

		/// <summary>
		/// Общая дебиторская задолженность, 0 - для физических лиц
		/// </summary>
		public decimal DebtorDebt { get; set; }
	}
}
