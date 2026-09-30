using CustomerOrdersApi.Library.V5.Dto.Orders.OrderItem;
using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;

namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	public interface ICreatingOnlineOrder
	{
		/// <summary>
		/// Источник заказа
		/// </summary>
		Source Source { get; set; }

		/// <summary>
		/// Номер онлайн заказа из ИПЗ
		/// </summary>
		Guid ExternalOrderId { get; set; }

		/// <summary>
		/// Id контрагента в ДВ
		/// </summary>
		int? CounterpartyErpId { get; set; }

		/// <summary>
		/// Контрольная сумма заказа, для проверки валидности отправителя
		/// </summary>
		string Signature { get; set; }

		/// <summary>
		/// Id клиента в ИПЗ
		/// </summary>
		Guid? ExternalCounterpartyId { get; set; }

		/// <summary>
		/// Id точки доставки в ДВ
		/// </summary>
		int? DeliveryPointId { get; set; }

		/// <summary>
		/// Самовывоз?
		/// </summary>
		bool IsSelfDelivery { get; set; }

		/// <summary>
		/// Id гео группы в ДВ для самовывоза
		/// </summary>
		int? SelfDeliveryGeoGroupId { get; set; }

		/// <summary>
		/// Форма оплаты
		/// </summary>
		OnlineOrderPaymentType OnlineOrderPaymentType { get; set; }

		/// <summary>
		/// Статус оплаты
		/// </summary>
		OnlineOrderPaymentStatus OnlineOrderPaymentStatus { get; set; }

		/// <summary>
		/// Номер оплаты
		/// </summary>
		int? OnlinePayment { get; set; }

		/// <summary>
		/// Источник оплаты
		/// </summary>
		OnlinePaymentSource? OnlinePaymentSource { get; set; }

		/// <summary>
		/// Нужно подтверждение по телефону?
		/// </summary>
		bool IsNeedConfirmationByCall { get; set; }

		/// <summary>
		/// Дата доставки
		/// </summary>
		DateTime DeliveryDate { get; set; }

		/// <summary>
		/// Id времени доставки из ДВ
		/// </summary>
		int? DeliveryScheduleId { get; set; }

		/// <summary>
		/// Отзвон за
		/// </summary>
		int? CallBeforeArrivalMinutes { get; set; }

		/// <summary>
		/// Доставка за час?
		/// </summary>
		bool IsFastDelivery { get; set; }

		/// <summary>
		/// Номер для связи
		/// </summary>
		string ContactPhone { get; set; }

		/// <summary>
		/// Комментарий к заказу
		/// </summary>
		string OnlineOrderComment { get; set; }

		/// <summary>
		/// Сдача с
		/// </summary>
		int? Trifle { get; set; }

		/// <summary>
		/// Бутылей на возврат
		/// </summary>
		int? BottlesReturn { get; set; }

		/// <summary>
		/// Сумма онлайн заказа
		/// </summary>
		decimal OrderSum { get; set; }

		/// <summary>
		/// Не приезжать раньше интервала
		/// </summary>
		bool DontArriveBeforeInterval { get; set; }

		/// <summary>
		/// Список товаров
		/// </summary>
		IList<OnlineOrderItemDto> OnlineOrderItems { get; set; }

		/// <summary>
		/// Список пакетов аренды
		/// </summary>
		IList<OnlineRentPackageDto> OnlineRentPackages { get; set; }
	}
}
