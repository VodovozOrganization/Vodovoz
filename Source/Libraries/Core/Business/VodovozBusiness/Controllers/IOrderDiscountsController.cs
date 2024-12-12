using System.Collections.Generic;
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
		/// Установка скидки исходя из выбранного основания скидки для строки счета без отгрузки на предоплату
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка счета без отгрузки на предоплату</param>
		void SetDiscountFromDiscountReasonForOrderItemWithoutShipment(
			DiscountReason reason, OrderWithoutShipmentForAdvancePaymentItem orderItem);
		/// <summary>
		/// Удаление скидки из заказа
		/// </summary>
		void RemoveDiscountFromOrder(IList<OrderItem> orderItems);
	}
}
