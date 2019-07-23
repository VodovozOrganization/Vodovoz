using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.Domain.Operations;

namespace Vodovoz.EntityRepositories.Cash
{
	public class CashRepository : ICashRepository
	{

		public decimal GetIncomePaidSumForOrder(IUnitOfWork uow, int orderId, int? excludedIncomeDoc = null)
		{
			var query = uow.Session.QueryOver<Income>().Where(x => x.Order.Id == orderId);
			if(excludedIncomeDoc != null) {
				query.Where(x => x.Id != excludedIncomeDoc);
			}
			return query.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();
		}

		public decimal GetExpenseReturnSumForOrder(IUnitOfWork uow, int orderId, int? excludedExpenseDoc = null)
		{
			var query = uow.Session.QueryOver<Expense>().Where(x => x.Order.Id == orderId);
			if(excludedExpenseDoc != null) {
				query.Where(x => x.Id != excludedExpenseDoc);
			}
			return query.Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();
		}

		public decimal CurrentCash (IUnitOfWork uow)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
				.Select (Projections.Sum<Expense> (o => o.Money)).SingleOrDefault<decimal> ();

			decimal income = uow.Session.QueryOver<Income>()
				.Select (Projections.Sum<Income> (o => o.Money)).SingleOrDefault<decimal> ();

			return income - expense;
		}

		public decimal CurrentCashForSubdivision(IUnitOfWork uow, Subdivision subdivision)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
				.Where(x => x.RelatedToSubdivision == subdivision)
				.Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();

			decimal income = uow.Session.QueryOver<Income>()
				.Where(x => x.RelatedToSubdivision == subdivision)
				.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();

			return income - expense;
		}

		public Income GetIncomeByRouteList(IUnitOfWork uow, int routeListId)
		{
			return uow.Session.QueryOver<Income>()
				.Where(inc => inc.RouteListClosing.Id == routeListId)
				.Where(inc => inc.TypeOperation == IncomeType.DriverReport)
				.Take(1).SingleOrDefault();
		}

		public Expense GetExpenseByRouteListId(IUnitOfWork uow, int routeListId)
		{
			return uow.Session.QueryOver<Expense>()
				.Where(exp => exp.RouteListClosing.Id == routeListId)
				.Where(exp => exp.TypeOperation == ExpenseType.Expense)
				.Take(1).SingleOrDefault();
		}

		public decimal CurrentRouteListCash(IUnitOfWork uow, int routeListId)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
			                     .Where(exp => exp.RouteListClosing.Id == routeListId)
			                     .Where(exp => exp.TypeOperation == ExpenseType.Expense)
								 .Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();

			decimal income = uow.Session.QueryOver<Income>()
			                    .Where(exp => exp.RouteListClosing.Id == routeListId)
			                    .Where(exp => exp.TypeOperation == IncomeType.DriverReport)
								.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();

			return income - expense;
		}

		/// <summary>
		/// Возвращает сумму находящуюся в перемещении между кассами
		/// </summary>
		public decimal GetCashInTransfering(IUnitOfWork uow)
		{
			CashTransferOperation cashTransferOperationAlias = null;
			CashTransferDocumentBase cashTransferDocumentAlias = null;
			return uow.Session.QueryOver<CashTransferDocumentBase>(() => cashTransferDocumentAlias)
				.Left.JoinAlias(() => cashTransferDocumentAlias.CashTransferOperation, () => cashTransferOperationAlias)
				.Where(() => cashTransferDocumentAlias.Status != CashTransferDocumentStatuses.Received)
				.Where(() => cashTransferDocumentAlias.Status != CashTransferDocumentStatuses.New)
				.Where(() => cashTransferOperationAlias.ReceiveTime == null)
				.Select(Projections.Sum<CashTransferOperation>(o => o.TransferedSum))
				.SingleOrDefault<decimal>();
		}
	}
}

