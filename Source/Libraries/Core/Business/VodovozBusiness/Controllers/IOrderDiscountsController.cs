using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Controllers
{
	public interface IOrderDiscountsController : IDiscountController
	{
		/// <summary>
		/// Устанавливает основание скидки плюс персональную скидку с введенными значениями в рублях или процентах для списка строк заказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="reason">Основание скидки</param>
		/// <param name="discountValue">Значение скидки</param>
		/// <param name="orderItems">Список строк заказа</param>
		void SetCustomDiscountForOrder(
			IUnitOfWork uow,
			DiscountReason reason,
			IDiscountValue discountValue,
			IEnumerable<IApplyDiscountReasonItem> orderItems);

		/// <summary>
		/// Установка скидки исходя из выбранного основания скидки для всего заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItems">Список строк заказа</param>
		/// <param name="canChangeDiscountValue">Может ли пользователь менять скидку</param>
		/// <param name="messages">Описание позиций на которые не применилась скидка</param>
		void SetDiscountFromDiscountReasonForOrder(
			DiscountReason reason,
			IEnumerable<IApplyDiscountReasonItem> orderItems,
			bool canChangeDiscountValue,
			out string messages);

		/// <summary>
		/// Установка скидки исходя из выбранного основания скидки для строки заказа
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка заказа</param>
		/// <param name="canChangeDiscountValue">Может ли пользователь менять скидку</param>
		/// <param name="message">Описание позици на которую не применилась скидка</param>
		/// <returns>true/false - установилась скидка или нет</returns>
		bool SetDiscountFromDiscountReasonForOrderItem(
			DiscountReason reason, IApplyDiscountReasonItem orderItem, bool canChangeDiscountValue, out string message);

		/// <summary>
		/// Добвляет скидку из выбранного основания скидки для строки заказа, если она не была установлена ранее
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="orderItem">Строка заказа</param>
		/// <param name="isNotCheckPromoSetOrFixedPrice">Можно добавить скидку независимо от наличия промонабора или фиксы</param>
		/// <returns>Результат операции</returns>
		Result AddDiscountFromDiscountReasonForOrderItem(DiscountReason reason, IApplyDiscountReasonItem orderItem, bool isNotCheckPromoSetOrFixedPrice = false);
		/// <summary>
		/// Удаление всех скидок из строк заказа
		/// </summary>
		/// <param name="orderItems">Список строк заказа</param>
		void ClearOrdersItemDiscounts(IList<IApplyDiscountReasonItem> orderItems);
		/// <summary>
		/// Сохранение текущих скидок в кэш или восстановление из него и пересчет по актуальным значениям
		/// </summary>
		/// <param name="saleItem">Позиция на продажу</param>
		void RecalculateDiscountWithPreserveOrRestoreDiscount(IPreserveDiscount saleItem);
		/// <summary>
		/// Если есть сохраненные скидки в кэше, то восстанавливаем их
		/// </summary>
		/// <param name="saleItem">Позиция на продажу</param>
		void TryRestoreOriginalDiscount(IPreserveDiscount saleItem);
	}
}
