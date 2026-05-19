using System;
using QS.Project.Journal;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.OnlineOrders;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class OnlineOrderTemplatesJournalNode : JournalEntityNodeBase
	{
		/// <summary>
		/// Заголовок(для EntityEntry, EntityViewModelEntry)
		/// </summary>
		public override string Title => $"{OnlineOrderTemplate.TemplateTitle} №{Id}";
		/// <summary>
		/// Клиент
		/// </summary>
		public string CounterpartyName { get; set; }
		/// <summary>
		/// Адрес доставки
		/// </summary>
		public string CompiledAddress { get; set; }
		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime DeliveryDate { get; set; }
		/// <summary>
		/// Телефон для связи
		/// </summary>
		public string ContactPhone { get; set; }
		/// <summary>
		/// Самовывоз
		/// </summary>
		public bool IsSelfDelivery { get; set; }
		/// <summary>
		/// Активен
		/// </summary>
		public bool IsActive { get; set; }
		/// <summary>
		/// В архиве
		/// </summary>
		public bool IsArchive { get; set; }
		/// <summary>
		/// Форма оплаты
		/// </summary>
		public OnlineOrderPaymentType PaymentType { get; set; }
		/// <summary>
		/// Периодичность доставки
		/// </summary>
		public OnlineOrderDeliveryFrequency DeliveryFrequency { get; set; }
		/// <summary>
		/// Время доставки
		/// </summary>
		public string DeliveryTime { get; set; }
		/// <summary>
		/// Дни недели
		/// </summary>
		public string Weekdays { get; set; }
		/// <summary>
		/// Номер последнего заказа
		/// </summary>
		public int? LastOrderId { get; set; }
		/// <summary>
		/// Статус
		/// </summary>
		public OnlineOrderTemplateStatus Status => IsActive ? OnlineOrderTemplateStatus.Active : OnlineOrderTemplateStatus.Inactive;
	}
}
