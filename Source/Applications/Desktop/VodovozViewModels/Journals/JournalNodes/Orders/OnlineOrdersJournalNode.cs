using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.Project.Journal;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.Extensions;
using Vodovoz.ViewModels.Journals.FilterViewModels.Enums;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Orders
{
	public class OnlineOrdersJournalNode : JournalEntityNodeBase
	{
		/// <summary>
		/// Заголовок(для EntityEntry, EntityViewModelEntry)
		/// </summary>
		public override string Title => string.Empty;
		/// <summary>
		/// Клиент
		/// </summary>
		public string CounterpartyName { get; set; }
		/// <summary>
		/// Тип сущности(онлайн заказ, заявка на звонок)
		/// </summary>
		public string EntityTypeString { get; set; }
		/// <summary>
		/// Автозаказ
		/// </summary>
		public bool IsFromTemplate { get; set; }
		/// <summary>
		/// Адрес доставки
		/// </summary>
		public string CompiledAddress { get; set; }
		/// <summary>
		/// Дата доставки
		/// </summary>
		public DateTime? DeliveryDate { get; set; }
		/// <summary>
		/// Дата создания
		/// </summary>
		public DateTime CreationDate { get; set; }
		/// <summary>
		/// Самовыоз
		/// </summary>
		public bool IsSelfDelivery { get; set; }
		/// <summary>
		/// Доставка за час
		/// </summary>
		public bool IsFastDelivery { get; set; }
		/// <summary>
		/// Время доставки
		/// </summary>
		public string DeliveryTime { get; set; }
		/// <summary>
		/// Статус онлайн заказа
		/// </summary>
		public OnlineOrderStatus? OnlineOrderStatus { get; set; }
		/// <summary>
		/// Статус заявки на звонок
		/// </summary>
		public RequestForCallStatus? RequestForCallStatus { get; set; }
		/// <summary>
		/// Значение для сортировки по статусу
		/// </summary>
		public int OrderByStatusValue { get; set; }

		/// <summary>
		/// Статус в зависимости от типа сущности
		/// </summary>
		public string Status
		{
			get
			{
				if(EntityType == typeof(OnlineOrder))
				{
					return OnlineOrderStatus.Value.GetEnumDisplayName();
				}

				if(EntityType == typeof(RequestForCall))
				{
					return RequestForCallStatus.Value.GetEnumDisplayName();
				}
				
				return string.Empty;
			}
		}

		/// <summary>
		/// В работе у пользователя
		/// </summary>
		public string ManagerWorkWith { get; set; }
		/// <summary>
		/// ИПЗ(источник получения заказа)
		/// </summary>
		public Core.Domain.Clients.Source Source { get; set; }
		/// <summary>
		/// Сумма заказа
		/// </summary>
		public decimal? OnlineOrderSum { get; set; }
		/// <summary>
		/// Статус оплаты заказа
		/// </summary>
		public OnlineOrderPaymentStatus? OnlineOrderPaymentStatus { get; set; }
		/// <summary>
		/// Номер оплаты
		/// </summary>
		public int? OnlinePayment { get; set; }
		/// <summary>
		/// Форма оплаты
		/// </summary>
		public OnlineOrderPaymentType OnlineOrderPaymentType { get; set; }
		/// <summary>
		/// Подтверждение по телефону
		/// </summary>
		public bool IsNeedConfirmationByCall { get; set; }
		/// <summary>
		/// Номера выставленных заказов
		/// </summary>
		public string OrdersIds { get; set; }
	}
}
