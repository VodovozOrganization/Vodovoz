using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;

namespace Vodovoz.EntityRepositories.Cash
{
	public interface ICashRepository
	{
		decimal CurrentCash(IUnitOfWork uow);
		decimal CurrentCashForSubdivision(IUnitOfWork uow, Subdivision subdivision);
		/// <summary>
		/// Возвращает баланс по предоставленным id касс
		/// </summary>
		IEnumerable<BalanceNode> CurrentCashForGivenSubdivisions(IUnitOfWork uow, int[] subdivisionIds);
		decimal CurrentRouteListCash(IUnitOfWork uow, int routeListId);
		decimal GetRouteListBalanceExceptAccountableCash(IUnitOfWork uow, int routeListId);
		decimal GetRouteListCashReturnSum(IUnitOfWork uow, int routeListId);
		decimal GetRouteListCashExpensesSum(IUnitOfWork uow, int routeListId);
		decimal GetCashInTransferring(IUnitOfWork uow);
		Expense GetExpenseByRouteListId(IUnitOfWork uow, int routeListId);
		decimal GetExpenseReturnSumForOrder(IUnitOfWork uow, int orderId, int? excludedExpenseDoc = null);
		Income GetIncomeByRouteList(IUnitOfWork uow, int routeListId);
		/// <summary>
		/// Возвращает сумму находящуюся в перемещении между кассами
		/// </summary>
		decimal GetIncomePaidSumForOrder(IUnitOfWork uow, int orderId, int? excludedIncomeDoc = null);
		bool OrderHasIncome(IUnitOfWork uow, int orderId);
		IList<OperationNode> GetCashBalanceForOrganizations(IUnitOfWork uow);
		decimal GetIncomeSumByRouteListId(IUnitOfWork uow, int routeListId, IncomeType[] includedIncomeTypes = null, IncomeType[] excludedIncomeTypes = null);
		decimal GetExpenseSumByRouteListId(IUnitOfWork uow, int routeListId, ExpenseType[] includedExpenseTypes = null, ExpenseType[] excludedExpenseTypes = null);
	}
}
