using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Interfaces;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Controllers
{
	public interface IDiscountController
	{
		/// <summary>
		/// Проверка применимости скидки к номенклатуре
		/// </summary>
		/// <param name="addingDiscount">Добавляемая скидка</param>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <returns>При успешном выполнении Result.Success, иначе Result.Failure с указанием проблемы</returns>
		Result IsApplicableDiscount(
			DiscountReason addingDiscount,
			IApplyDiscountReasonItem saleItem
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
		/// 
		/// </summary>
		/// <param name="saleItem"></param>
		/// <param name="newDiscount">Новая скидка, установленная вручную</param>
		/// <param name="context">Данные для пересчета скидки</param>
		//void RecalculateDiscount(IApplyDiscountReasonItem saleItem, IDiscountValue newDiscount = null);
		void RecalculateDiscount(IDataContext context);
	}
}
