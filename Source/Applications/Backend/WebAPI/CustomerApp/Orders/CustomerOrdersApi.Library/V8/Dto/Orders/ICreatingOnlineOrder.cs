using System;
using System.Collections.Generic;
using CustomerOrdersApi.Library.V8.Dto.Orders.OrderItem;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V8.Dto.Orders
{
	/// <summary>
	/// Информация для создания онлайн заказа из ИПЗ
	/// </summary>
	public interface ICreatingOnlineOrder
	{
		/// <summary>
		/// Источник заказа
		/// </summary>
		Source Source { get; }

		/// <summary>
		/// Номер онлайн заказа из ИПЗ
		/// </summary>
		Guid ExternalOrderId { get; }

		/// <summary>
		/// Id контрагента в ДВ
		/// </summary>
		int? ErpCounterpartyId { get; }

		/// <summary>
		/// Контрольная сумма заказа, для проверки валидности отправителя
		/// </summary>
		string Signature { get; }

		/// <summary>
		/// Id клиента в ИПЗ
		/// </summary>
		Guid? ExternalCounterpartyId { get; }

		/// <summary>
		/// Id точки доставки в ДВ
		/// </summary>
		int? DeliveryPointId { get; }

		/// <summary>
		/// Самовывоз?
		/// </summary>
		bool IsSelfDelivery { get; }

		/// <summary>
		/// Id гео группы в ДВ для самовывоза
		/// </summary>
		int? SelfDeliveryGeoGroupId { get; }

		/// <summary>
		/// Форма оплаты
		/// </summary>
		OnlineOrderPaymentType OnlineOrderPaymentType { get; }

		/// <summary>
		/// Статус оплаты
		/// </summary>
		OnlineOrderPaymentStatus OnlineOrderPaymentStatus { get; }

		/// <summary>
		/// Номер оплаты
		/// </summary>
		int? OnlinePayment { get; }

		/// <summary>
		/// Источник оплаты
		/// </summary>
		OnlinePaymentSource? OnlinePaymentSource { get; }

		/// <summary>
		/// Нужно подтверждение по телефону?
		/// </summary>
		bool IsNeedConfirmationByCall { get; }

		/// <summary>
		/// Дата доставки
		/// </summary>
		DateTime DeliveryDate { get; }

		/// <summary>
		/// Id времени доставки из ДВ
		/// </summary>
		int? DeliveryScheduleId { get; }

		/// <summary>
		/// Отзвон за
		/// </summary>
		int? CallBeforeArrivalMinutes { get; }

		/// <summary>
		/// Доставка за час?
		/// </summary>
		bool IsFastDelivery { get; }

		/// <summary>
		/// Номер для связи
		/// </summary>
		string ContactPhone { get; }

		/// <summary>
		/// Комментарий к заказу
		/// </summary>
		string OnlineOrderComment { get; }

		/// <summary>
		/// Сдача с
		/// </summary>
		int? Trifle { get; }

		/// <summary>
		/// Бутылей на возврат
		/// </summary>
		int? BottlesReturn { get; }

		/// <summary>
		/// Сумма онлайн заказа
		/// </summary>
		decimal OrderSum { get; }

		/// <summary>
		/// Не приезжать раньше интервала
		/// </summary>
		bool DontArriveBeforeInterval { get; }
						
		/// <summary>
		/// Автозаказ подключен
		/// </summary>
		bool IsAutoOrderEnabled { get; }

		/// <summary>
		/// Список товаров
		/// </summary>
		IList<OnlineOrderItemDto> OnlineOrderItems { get; }
	}
}
