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
		/// <param name="UoW">UoW</param>
		/// <param name="orderByDescending">Если <c>true</c>, то сортируется список по убыванию.</param>
		IList<DiscountReason> GetDiscountReasons(IUnitOfWork UoW, bool orderByDescending = false);
		IList<DiscountReason> GetActiveDiscountReasons(IUnitOfWork uow);
		IList<DiscountReason> GetActiveDiscountReasonsWithoutPremiums(IUnitOfWork uow);
		bool ExistsActiveDiscountReasonWithName(IUnitOfWork uow, int discountReasonId, string name, out DiscountReason discountReason);
		DiscountReason GetActivePromoCode(IUnitOfWork uow, string promoCode);
		bool HasBeenUsagePromoCode(IUnitOfWork uow, int counterpartyId, int discountReasonId);
		bool ExistsPromoCodeWithName(IUnitOfWork uow, int discountReasonId, string promoCode, out DiscountReason discountReason);
	}
}
