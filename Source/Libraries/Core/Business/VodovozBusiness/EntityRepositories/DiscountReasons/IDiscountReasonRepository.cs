using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.EntityRepositories.DiscountReasons
{
	public interface IDiscountReasonRepository
	{
		/// <summary>
		/// Возврат отсортированного списка скидок
		/// </summary>
		/// <returns>Список скидок</returns>
		/// <param name="uow">unit of work</param>
		/// <param name="orderByDescending">Если <c>true</c>, то сортируется список по убыванию.</param>
		IList<DiscountReasonBase> GetDiscountReasons(IUnitOfWork uow, bool orderByDescending = false);
		/// <summary>
		/// Получение списка скидок по переданным идентификаторам
		/// </summary>
		/// <returns>Список скидок</returns>
		/// <param name="uow">unit of work</param>
		/// <param name="discountReasonIds">Список подбираемых оснований</param>
		IEnumerable<DiscountReasonBase> GetDiscountReasons(IUnitOfWork uow, IEnumerable<int> discountReasonIds);
		IList<DiscountReasonBase> GetActiveDiscountReasons(IUnitOfWork uow);
		IList<DiscountReasonBase> GetActiveDiscountReasonsWithoutPremiums(IUnitOfWork uow);
		/// <summary>
		/// Возвращает список оснований для скидки. При этом подгружаются связанные сущности
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="canChoosePremiumDiscount">Если <c>true</c>, то в список будут включены основания для премиальных скидок</param>
		/// <returns>Список оснований для скидки</returns>
		IList<DiscountReasonBase> GetActiveDiscountReasonsFetchReferences(IUnitOfWork uow, bool canChoosePremiumDiscount);
		bool ExistsActiveDiscountReasonWithName(IUnitOfWork uow, int discountReasonId, string name, out DiscountReasonBase discountReason);
		PromoCodeDiscount GetActivePromoCode(IUnitOfWork uow, string promoCode);
		bool HasBeenUsagePromoCode(IUnitOfWork uow, int? counterpartyId, int discountReasonId);
		bool ExistsPromoCodeWithName(IUnitOfWork uow, int discountReasonId, string promoCode, out PromoCodeDiscount discountReason);
		/// <summary>
		/// Получение основания скидки по id
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="discountReasonId">Идентификатор основания скидки</param>
		/// <returns></returns>
		DiscountReasonBase GetDiscountReason(IUnitOfWork uow, int discountReasonId);
	}
}
