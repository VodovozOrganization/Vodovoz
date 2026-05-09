using System.Collections.Generic;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using VodovozBusiness.Controllers;

namespace Vodovoz.Controllers
{
	public interface IOrderDiscountsController : IDiscountController
	{
		/// <summary>
		/// Устанавливает основание скидки с введенными значениями в рублях или процентах для всего заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="discount">Скидки</param>
		/// <param name="unit">Скидка в процентах или рублях</param>
		/// <param name="orderItems">Список строк заказа</param>
		void SetCustomDiscountForOrder(DiscountReason reason, decimal discount, DiscountUnits unit, IList<OrderItem> orderItems);

		/// <summary>
		/// Установка скидки исходя из выбранного основания скидки для всего заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItems">Список строк заказа</param>
		/// <param name="canChangeDiscountValue">Может ли пользователь менять скидку</param>
		/// <param name="messages">Описание позиций на которые не применилась скидка</param>
		void SetDiscountFromDiscountReasonForOrder(
			DiscountReason reason, IList<OrderItem> orderItems, bool canChangeDiscountValue, out string messages);

		/// <summary>
		/// Установка скидки исходя из выбранного основания скидки для строки заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка заказа</param>
		/// <param name="canChangeDiscountValue">Может ли пользователь менять скидку</param>
		/// <param name="message">Описание позици на которую не применилась скидка</param>
		/// <returns>true/false - установилась скидка или нет</returns>
		bool SetDiscountFromDiscountReasonForOrderItem(
			DiscountReason reason, OrderItem orderItem, bool canChangeDiscountValue, out string message);

		/// <summary>
		/// Добвляет скидку из выбранного основания скидки для строки заказа, если она не была установлена ранее
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка заказа</param>
		/// <returns>Результат операции</returns>
		Result AddtDiscountFromDiscountReasonForOrderItem(DiscountReason reason, OrderItem orderItem);

		/// <summary>
		/// Установка скидки исходя из выбранного основания скидки для строки счета без отгрузки на предоплату
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка счета без отгрузки на предоплату</param>
		void SetDiscountFromDiscountReasonForOrderItemWithoutShipment(
			DiscountReason reason, OrderWithoutShipmentForAdvancePaymentItem orderItem);

		/// <summary>
		/// Удаление всех скидок из строк заказа
		/// </summary>
		/// <param name="orderItems">Список строк заказа</param>
		void ClearOrdersItemDiscounts(IList<OrderItem> orderItems);

		/// <summary>
		/// Удаление указанной скидки из строк заказа
		/// </summary>
		/// <param name="discountReason">Основание скидки</param>
		/// <param name="orderItem">Строка заказа</param>
		void RemoveOrdersItemDiscounts(DiscountReason discountReason, OrderItem orderItem);
	}
}
