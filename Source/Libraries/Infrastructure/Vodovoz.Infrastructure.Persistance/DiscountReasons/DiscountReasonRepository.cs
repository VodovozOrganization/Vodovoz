using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.DiscountReasons;

namespace Vodovoz.Infrastructure.Persistance.DiscountReasons
{
	internal sealed class DiscountReasonRepository : IDiscountReasonRepository
	{
		/// <summary>
		/// Возврат отсортированного списка скидок
		/// </summary>
		/// <returns>Список скидок</returns>
		/// <param name="UoW">UoW</param>
		/// <param name="orderByDescending">Если <c>true</c>, то сортируется список по убыванию имени скидки</param>
		public IList<DiscountReason> GetDiscountReasons(IUnitOfWork UoW, bool orderByDescending = false)
		{
			var query = UoW.Session.QueryOver<DiscountReason>()
				.OrderBy(i => i.Name);
			return orderByDescending ? query.Desc().List() : query.Asc().List();
		}

		public IList<DiscountReason> GetActiveDiscountReasons(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<DiscountReason>()
				.WhereNot(dr => dr.IsArchive)
				.OrderBy(dr => dr.Name)
				.Asc()
				.List();
		}

		public IList<DiscountReason> GetActiveDiscountReasonsWithoutPremiums(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<DiscountReason>()
				.Where(dr => !dr.IsArchive)
				.And(dr => !dr.IsPremiumDiscount)
				.OrderBy(dr => dr.Name)
				.Asc()
				.List();
		}

		public bool ExistsActiveDiscountReasonWithName(
			IUnitOfWork uow, int discountReasonId, string name, out DiscountReason discountReason)
		{
			discountReason = uow.Session.QueryOver<DiscountReason>()
				.Where(dr => !dr.IsArchive)
				.And(dr => dr.Id != discountReasonId)
				.And(dr => dr.Name == name)
				.SingleOrDefault();

			return discountReason != null;
		}
	}
}
