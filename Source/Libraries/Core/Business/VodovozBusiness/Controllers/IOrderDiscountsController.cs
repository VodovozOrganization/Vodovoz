using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Controllers;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Controllers
{
	public interface IOrderDiscountsController : ISaleDiscountController
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
			DiscountReasonBase reason,
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
			DiscountReasonBase reason,
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
			DiscountReasonBase reason, IApplyDiscountReasonItem orderItem, bool canChangeDiscountValue, out string message);

		/// <summary>
		/// "Тихая" установка скидки с основанием(не использовать без особой необходимости)
		/// </summary>
		/// <param name="discountReason">Основание скидки</param>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <param name="discountValue">Значение скидки</param>
		void SilentSetCustomDiscount(
			DiscountReasonBase discountReason,
			IApplyDiscountReasonItem saleItem,
			IDiscountValue discountValue
		);
		
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
		/// <summary>
		/// Обновление скидки по второму заказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="source">Источник(заказ)</param>
		OkResult UpdateClientSecondOrderDiscount(IUnitOfWork uow, ISecondOrderDiscount source);
		/// <summary>
		/// Копирование скидок из переданной позиции
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Позиция на продажу, куда копируются скидки</param>
		/// <param name="copyingSaleItem">Позиция на продажу из которой копируются скидки</param>
		void CopyDiscounts(IUnitOfWork uow, IApplyDiscountReasonItem saleItem, IApplyDiscountReasonItem copyingSaleItem);
		/// <summary>
		/// Копирование кэшированных скидок из переданной позиции
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Позиция на продажу, куда копируются скидк</param>
		/// <param name="copyingSaleItem">Позиция на продажу из которой копируются скидки</param>
		void CopyOriginalDiscounts(IUnitOfWork uow, IApplyDiscountReasonItem saleItem, IPreserveDiscount copyingSaleItem);
	}
}
