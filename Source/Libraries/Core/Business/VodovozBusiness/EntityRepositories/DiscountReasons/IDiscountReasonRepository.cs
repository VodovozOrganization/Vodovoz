using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;

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
		IList<DiscountReason> GetDiscountReasons(IUnitOfWork uow, bool orderByDescending = false);
		/// <summary>
		/// Получение списка скидок по переданным идентификаторам
		/// </summary>
		/// <returns>Список скидок</returns>
		/// <param name="uow">unit of work</param>
		/// <param name="discountReasonIds">Список подбираемых оснований</param>
		IEnumerable<DiscountReason> GetDiscountReasons(IUnitOfWork uow, IEnumerable<int> discountReasonIds);
		IList<DiscountReason> GetActiveDiscountReasons(IUnitOfWork uow);
		IList<DiscountReason> GetActiveDiscountReasonsWithoutPremiums(IUnitOfWork uow);
		/// <summary>
		/// Возвращает список оснований для скидки. При этом подгружаются связанные сущности
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="canChoosePremiumDiscount">Если <c>true</c>, то в список будут включены основания для премиальных скидок</param>
		/// <returns>Список оснований для скидки</returns>
		IList<DiscountReason> GetActiveDiscountReasonsFetchReferences(IUnitOfWork uow, bool canChoosePremiumDiscount);
		bool ExistsActiveDiscountReasonWithName(IUnitOfWork uow, int discountReasonId, string name, out DiscountReason discountReason);
		DiscountReason GetActivePromoCode(IUnitOfWork uow, string promoCode);
		/// <summary>
		/// Проверка, есть ли продажи с таким промокодом
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="counterpartyId">Идентификатор клиента</param>
		/// <param name="discountReasonId">Идентификатор промокода</param>
		/// <returns></returns>
		bool HasBeenUsagePromoCode(IUnitOfWork uow, int? counterpartyId, int discountReasonId);
		bool ExistsPromoCodeWithName(IUnitOfWork uow, int discountReasonId, string promoCode, out DiscountReason discountReason);
	}
}
