using System;
using System.Collections.Generic;
using System.Linq;

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
		public int OrganizationId { get; set; }

		/// <summary>
		/// Наименование организации
		/// </summary>
		public string OrganizationName { get; set; }

		/// <summary>
		/// Id контрагента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Наименование контрагента
		/// </summary>
		public string CounterpartyName { get; set; }

		/// <summary>
		/// ИНН контрагента
		/// </summary>
		public string CounterpartyInn { get; set; }

		public IEnumerable<string> CounterpartyPhones { get; set; } = Enumerable.Empty<string>();
		public IEnumerable<string> CounterpartyOrdersContactPhones { get; set; } = Enumerable.Empty<string>();

		/// <summary>
		/// Номер телефона контрагента
		/// </summary>
		public string PhoneNumber => CounterpartyPhones.Any()
			? string.Join(", ", CounterpartyPhones)
			: string.Join(", ", CounterpartyOrdersContactPhones);

		/// <summary>
		/// Нераспределенный баланс
		/// </summary>
		public decimal UnallocatedBalance { get; set; }

		/// <summary>
		/// Сумма неоплаченных заказов
		/// </summary>
		public decimal NotPaidOrdersSum { get; set; }

		/// <summary>
		/// Сумма частичной оплаты
		/// </summary>
		public decimal PartialPaidOrdersSum { get; set; }

		/// <summary>
		/// Возвращенный баланс
		/// </summary>
		public decimal WriteOffSum { get; set; }

		/// <summary>
		/// Общий долг
		/// </summary>
		public decimal TotalDebt => NotPaidOrdersSum - UnallocatedBalance - PartialPaidOrdersSum;

		/// <summary>
		/// Дебиторская задолженность
		/// </summary>
		public decimal DebtorDebt => NotPaidOrdersSum - UnallocatedBalance - PartialPaidOrdersSum - OverdueDebtorDebt;

		/// <summary>
		/// Просроченная дебиторская задолженность
		/// </summary>
		public decimal OverdueDebtorDebt { get; set; }

		/// <summary>
		/// Отсрочка по оплате для контрагента в днях
		/// </summary>
		public int DelayDaysForCounterparty { get; set; }

		/// <summary>
		/// Дата доставки самого давнего заказа
		/// </summary>
		public DateTime? OrderMinDeliveryDate { get; set; }

		/// <summary>
		/// Максимальное количество дней просрочки
		/// </summary>
		public int MaxDelayDays =>
			OrderMinDeliveryDate.HasValue
			? (DateTime.Now - OrderMinDeliveryDate.Value).Days
			: 0;

		/// <summary>
		/// Контрагент в статусе ликвидации
		/// </summary>
		public bool IsCounterpartyLiquidating { get; set; }

		/// <summary>
		/// Контрагент в статусе ликвидации
		/// "Да" - если ликвидирован
		/// "Нет" - если не ликвидирован
		/// </summary>
		public string LiquidationStatus =>
			IsCounterpartyLiquidating
			? "Да"
			: "Нет";

		/// <summary>
		/// Дата и время выгрузки данных
		/// </summary>
		public DateTime UnloadingDate =>
			DateTime.Today;

		public int BottlesDelivered { get; set; }
		public int BottlesReturned { get; set; }

		/// <summary>
		/// Долг по бутылям
		/// </summary>
		public int BottlesDebt => BottlesDelivered - BottlesReturned;

		/// <summary>
		/// Email адреса
		/// </summary>
		public IEnumerable<string> Emails { get; set; }
	}
}
