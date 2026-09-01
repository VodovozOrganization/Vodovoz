using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Controllers
{
	public interface ISaleDiscountController : IDiscountController
	{
		/// <summary>
		/// Добавляет скидку из выбранного основания скидки для позиции на продажу, если она не была установлена ранее
		/// </summary>
		/// <param name="reason">Основание скидки</param>
		/// <param name="saleItem">Позиция на продажу</param>
		/// <param name="isNotCheckPromoSetOrFixedPrice">Можно добавить скидку независимо от наличия промонабора или фиксы</param>
		/// <returns>Результат операции</returns>
		Result AddDiscountFromDiscountReason(
			DiscountReasonBase reason,
			IApplyDiscountReasonItem saleItem,
			bool isNotCheckPromoSetOrFixedPrice = false
		);
		
		/// <summary>
		/// Установка индивидуальной скидки(при изменении параметров скидки в ДВ)
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Позиция на продажу, к которой применяется скидка</param>
		/// <param name="receivedDiscountValue">Установленное значение скидки</param>
		/// <returns><see cref="Result"/></returns>
		Result SetCustomDiscount(
			IUnitOfWork uow,
			IApplyDiscountReasonItem saleItem,
			IDiscountValue receivedDiscountValue
		);

		/// <summary>
		/// Удаление скидки из позиции
		/// </summary>
		/// <param name="discountReasonId">Идентификатор скидки</param>
		/// <param name="saleItem">Продаваемая позиция</param>
		void RemoveDiscount(int discountReasonId, IApplyDiscountReasonItem saleItem);

		/// <summary>
		/// Пересчет скидок
		/// </summary>
		/// <param name="context">Данные для пересчета скидки</param>
		void RecalculateDiscount(IDataContext context);

		/// <summary>
		/// Проверяет, что скидка может быть добавлена, т.е. сумма всех добавленных скидок не превышает цену товара
		/// </summary>
		/// <param name="discountValue">Значение скидки</param>
		/// <param name="saleItem">Позиция на которую применяется скидка</param>
		/// <returns>Результат проверки</returns>
		bool IsDiscountValueCanBeAdded(IDiscountValue discountValue, IApplyDiscountReasonItem saleItem);
	}
}
