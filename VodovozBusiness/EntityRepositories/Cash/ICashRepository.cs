using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;

namespace Vodovoz.EntityRepositories.Cash
{
	public interface ICashRepository
	{
		decimal CurrentCash(IUnitOfWork uow);
		decimal CurrentCashForSubdivision(IUnitOfWork uow, Subdivision subdivision);
		decimal CurrentRouteListCash(IUnitOfWork uow, int routeListId);
		decimal GetCashInTransfering(IUnitOfWork uow);
		Expense GetExpenseByRouteListId(IUnitOfWork uow, int routeListId);
		decimal GetExpenseReturnSumForOrder(IUnitOfWork uow, int orderId, int? excludedExpenseDoc = null);
		Income GetIncomeByRouteList(IUnitOfWork uow, int routeListId);
		/// <summary>
		/// Возвращает сумму находящуюся в перемещении между кассами
		/// </summary>
		decimal GetIncomePaidSumForOrder(IUnitOfWork uow, int orderId, int? excludedIncomeDoc = null);
		bool OrderHasIncome(IUnitOfWork uow, int orderId);
	}
}