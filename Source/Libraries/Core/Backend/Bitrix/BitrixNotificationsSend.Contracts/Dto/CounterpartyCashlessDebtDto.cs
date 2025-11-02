using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Данные о контрагенте и его финансовых показателях
	/// </summary>
	public class CounterpartyCashlessDebtDto
	{
		/// <summary>
		/// Id организации
		/// </summary>
		[JsonPropertyName("organizationId")]
		public int OrganizationId { get; set; }

		/// <summary>
		/// Наименование организации
		/// </summary>
		[JsonPropertyName("organization")]
		public string OrganizationName { get; set; }

		/// <summary>
		/// Id контрагента
		/// </summary>
		[JsonIgnore]
		public int CounterpartyId { get; set; }

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
		/// Номера телефонов контрагента
		/// </summary>
		[JsonIgnore]
		public IEnumerable<string> CounterpartyPhones { get; set; } = Enumerable.Empty<string>();

		/// <summary>
		/// Номера телефонов контактных лиц по заказам контрагента
		/// </summary>
		[JsonIgnore]
		public IEnumerable<string> CounterpartyOrdersContactPhones { get; set; } = Enumerable.Empty<string>();

		/// <summary>
		/// Номер телефона контрагента
		/// </summary>
		[JsonPropertyName("phoneNumber")]
		public string PhoneNumbers => CounterpartyPhones.Any()
			? string.Join(", ", CounterpartyPhones)
			: string.Join(", ", CounterpartyOrdersContactPhones);

		/// <summary>
		/// Нераспределенный баланс
		/// </summary>
		[JsonPropertyName("unallocatedBalance")]
		public decimal UnallocatedBalance { get; set; }

		/// <summary>
		/// Сумма неоплаченных заказов
		/// </summary>
		[JsonPropertyName("amountUnpaidOrders")]
		public decimal NotPaidOrdersSum { get; set; }

		/// <summary>
		/// Сумма частичной оплаты
		/// </summary>
		[JsonPropertyName("amountPatrialPayment")]
		public decimal PartialPaidOrdersSum { get; set; }

		/// <summary>
		/// Возвращенный баланс
		/// </summary>
		[JsonPropertyName("returnedBalance")]
		public decimal WriteOffSum { get; set; }

		/// <summary>
		/// Общий долг
		/// </summary>
		[JsonPropertyName("totalDebt")]
		public decimal TotalDebt => NotPaidOrdersSum - UnallocatedBalance - PartialPaidOrdersSum;

		/// <summary>
		/// Дебиторская задолженность
		/// </summary>
		[JsonPropertyName("accountsReceivable")]
		public decimal DebtorDebt => NotPaidOrdersSum - UnallocatedBalance - PartialPaidOrdersSum - OverdueDebtorDebt;

		/// <summary>
		/// Просроченная дебиторская задолженность
		/// </summary>
		[JsonPropertyName("overdueAccountsReceivable")]
		public decimal OverdueDebtorDebt { get; set; }

		/// <summary>
		/// Отсрочка по оплате для контрагента в днях
		/// </summary>
		[JsonPropertyName("daysOfDeferment")]
		public int DelayDaysForCounterparty { get; set; }

		/// <summary>
		/// Дата доставки самого давнего заказа
		/// </summary>
		[JsonIgnore]
		public DateTime? OrderMinDeliveryDate { get; set; }

		/// <summary>
		/// Максимальное количество дней просрочки
		/// </summary>
		[JsonPropertyName("maxDaysOverdue")]
		public int MaxDelayDays =>
			OrderMinDeliveryDate.HasValue
			? (DateTime.Now - OrderMinDeliveryDate.Value).Days
			: 0;

		/// <summary>
		/// Контрагент в статусе ликвидации
		/// </summary>
		[JsonIgnore]
		public bool IsCounterpartyLiquidating { get; set; }

		/// <summary>
		/// Контрагент в статусе ликвидации
		/// "Да" - если ликвидирован
		/// "Нет" - если не ликвидирован
		/// </summary>
		[JsonPropertyName("liquidationStatus")]
		public string LiquidationStatus =>
			IsCounterpartyLiquidating
			? "Да"
			: "Нет";

		/// <summary>
		/// Дата и время выгрузки данных
		/// </summary>
		[JsonPropertyName("unloadingDate")]
		public DateTime UnloadingDate =>
			DateTime.Today;

		/// <summary>
		/// Бутыли доставленные
		/// </summary>
		[JsonIgnore]
		public int BottlesDelivered { get; set; }

		/// <summary>
		/// Бутыли возвращенные
		/// </summary>
		[JsonIgnore]
		public int BottlesReturned { get; set; }

		/// <summary>
		/// Долг по бутылям
		/// </summary>
		[JsonPropertyName("bottleDebt")]
		public int BottlesDebt => BottlesDelivered - BottlesReturned;

		/// <summary>
		/// Email адреса
		/// </summary>
		[JsonIgnore]
		public IEnumerable<string> Emails { get; set; } = Enumerable.Empty<string>();

		/// <summary>
		/// Email адреса
		/// </summary>
		[JsonPropertyName("emailAdress")]
		public string EmailAddresses => Emails != null && Emails.Any()
			? string.Join(", ", Emails)
			: string.Empty;
	}
}
