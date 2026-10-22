using System;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.EntityRepositories.Nodes
{
	/// <summary>
	/// Данные заказа сети или тендерного клиента в окне уведомления по правилам отчета "Отсрочка сети"
	/// </summary>
	public class ChainStoreDelayOrderNode
	{
		/// <summary>
		/// Id заказа
		/// </summary>
		public int OrderId { get; set; }

		/// <summary>
		/// Id клиента
		/// </summary>
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Наименование клиента
		/// </summary>
		public string CounterpartyName { get; set; }

		/// <summary>
		/// ИНН клиента
		/// </summary>
		public string CounterpartyInn { get; set; }

		/// <summary>
		/// Дата доставки заказа
		/// </summary>
		public DateTime DeliveryDate { get; set; }

		/// <summary>
		/// Отсрочка платежа клиента в днях
		/// </summary>
		public int DelayDaysForBuyers { get; set; }

		/// <summary>
		/// Признак самовывоза
		/// </summary>
		public bool IsSelfDelivery { get; set; }

		/// <summary>
		/// Короткий адрес точки доставки
		/// </summary>
		public string DeliveryPointShortAddress { get; set; }

		/// <summary>
		/// Фамилия автора заказа
		/// </summary>
		public string AuthorLastName { get; set; }

		/// <summary>
		/// Имя автора заказа
		/// </summary>
		public string AuthorName { get; set; }

		/// <summary>
		/// Отчество автора заказа
		/// </summary>
		public string AuthorPatronymic { get; set; }

		/// <summary>
		/// Номер для связи, указанный в заказе
		/// </summary>
		public string ContactPhoneNumber { get; set; }

		/// <summary>
		/// Статус оплаты заказа
		/// </summary>
		public OrderPaymentStatus OrderPaymentStatus { get; set; }

		/// <summary>
		/// Сумма заказа
		/// </summary>
		public decimal OrderSum { get; set; }

		/// <summary>
		/// Оплаченная сумма по заказу
		/// </summary>
		public decimal PaidSum { get; set; }

		/// <summary>
		/// Расчетная дата платежа: дата доставки плюс отсрочка клиента
		/// </summary>
		public DateTime EstimatedPaymentDate => DeliveryDate.Date.AddDays(DelayDaysForBuyers);

		/// <summary>
		/// Долг по заказу: для частично оплаченного заказа сумма за вычетом оплаченного
		/// </summary>
		public decimal Debt => OrderPaymentStatus == OrderPaymentStatus.PartiallyPaid ? OrderSum - PaidSum : OrderSum;

		/// <summary>
		/// Автор заказа в формате "Фамилия И.О."; пусто, если фамилия автора не указана
		/// </summary>
		public string AuthorShortName =>
			string.IsNullOrEmpty(AuthorLastName)
				? string.Empty
				: $"{AuthorLastName} {FirstLetter(AuthorName)}.{FirstLetter(AuthorPatronymic)}.";

		private static string FirstLetter(string value)
		{
			return string.IsNullOrEmpty(value) ? string.Empty : value.Substring(0, 1);
		}
	}
}
