using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Repository.Cash
{
	[Obsolete("Используйте одноимённый класс из Vodovoz.EntityRepositories.Cash")]
	public static class CashRepository
	{
		[Obsolete]
		public static decimal GetIncomePaidSumForOrder(IUnitOfWork uow, int orderId, int? excludedIncomeDoc = null)
		{
			return new EntityRepositories.Cash.CashRepository().GetIncomePaidSumForOrder(uow, orderId, excludedIncomeDoc);
		}

		[Obsolete]
		public static decimal GetExpenseReturnSumForOrder(IUnitOfWork uow, int orderId, int? excludedExpenseDoc = null)
		{
			return new EntityRepositories.Cash.CashRepository().GetExpenseReturnSumForOrder(uow, orderId, excludedExpenseDoc);
		}

		[Obsolete]
		public static decimal CurrentCash(IUnitOfWork uow)
		{
			return new EntityRepositories.Cash.CashRepository().CurrentCash(uow);
		}

		[Obsolete]
		public static decimal CurrentCashForSubdivision(IUnitOfWork uow, Subdivision subdivision)
		{
			return new EntityRepositories.Cash.CashRepository().CurrentCashForSubdivision(uow, subdivision);
		}

		[Obsolete]
		public static Income GetIncomeByRouteList(IUnitOfWork uow, int routeListId)
		{
			return new EntityRepositories.Cash.CashRepository().GetIncomeByRouteList(uow, routeListId);
		}

		[Obsolete]
		public static Expense GetExpenseByRouteListId(IUnitOfWork uow, int routeListId)
		{
			return new EntityRepositories.Cash.CashRepository().GetExpenseByRouteListId(uow, routeListId);
		}

		[Obsolete]
		public static decimal CurrentRouteListCash(IUnitOfWork uow, int routeListId)
		{
			return new EntityRepositories.Cash.CashRepository().CurrentRouteListCash(uow, routeListId);
		}

		[Obsolete]
		public static decimal GetCashInTransfering(IUnitOfWork uow)
		{
			return new EntityRepositories.Cash.CashRepository().GetCashInTransfering(uow);
		}
	}
}

