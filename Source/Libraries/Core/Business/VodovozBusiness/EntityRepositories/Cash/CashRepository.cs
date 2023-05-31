using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Organizations;

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
		
		public bool OrderHasIncome(IUnitOfWork uow, int orderId) {
			var query = uow.Session.QueryOver<Income>()
			               .Where(x => x.Order.Id == orderId)
			               .List();

			return query.Any();
		}

		public decimal CurrentCash (IUnitOfWork uow)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
				.Select (Projections.Sum<Expense> (o => o.Money)).SingleOrDefault<decimal> ();

			decimal income = uow.Session.QueryOver<Income>()
				.Select (Projections.Sum<Income> (o => o.Money)).SingleOrDefault<decimal> ();

			return income - expense;
		}
		
		public decimal CurrentCashForOrganization (IUnitOfWork uow, Organization organization)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
				.Where(x => x.Organisation == organization)
				.Select (Projections.Sum<Expense> (o => o.Money)).SingleOrDefault<decimal> ();

			decimal income = uow.Session.QueryOver<Income>()
				.Where(x => x.Organisation == organization)
				.Select (Projections.Sum<Income> (o => o.Money)).SingleOrDefault<decimal> ();

			return income - expense;
		}

		public IList<OperationNode> GetCashBalanceForOrganizations(IUnitOfWork uow)
		{
			Organization organizationAlias = null;
			OrganisationCashMovementOperation operationAlias = null;
			OperationNode resultAlias = null;

			var query = uow.Session.QueryOver(() => organizationAlias)
				.JoinEntityAlias(() => operationAlias, 
					() => organizationAlias.Id == operationAlias.Organisation.Id,
					JoinType.LeftOuterJoin
				)
				.SelectList(list => list
					.SelectGroup(() => organizationAlias.Id)
					.Select(Projections.Sum(() => operationAlias.Amount)).WithAlias(() => resultAlias.Balance)
					.Select(() => organizationAlias.Name).WithAlias(() => resultAlias.Name))
				.TransformUsing(Transformers.AliasToBean<OperationNode>())
				.List<OperationNode>();

			return query;
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

		public IEnumerable<BalanceNode> CurrentCashForGivenSubdivisions(IUnitOfWork uow, int[] subdivisionIds)
		{
			Subdivision subdivisionAlias = null;
			Income incomeAlias = null;
			Expense expenseAlias = null;
			BalanceNode resultAlias = null;

			var expenseSub = QueryOver.Of(() => expenseAlias)
				.Where(x => x.RelatedToSubdivision.Id == subdivisionAlias.Id)
				.Select(Projections.Sum<Expense>(o => o.Money));

			var incomeSub = QueryOver.Of(() => incomeAlias)
				.Where(x => x.RelatedToSubdivision.Id == subdivisionAlias.Id)
				.Select(Projections.Sum<Income>(o => o.Money));

			var projection = Projections.SqlFunction(
				new SQLFunctionTemplate(NHibernateUtil.Decimal, "( IFNULL(?1, 0) - IFNULL(?2, 0) )"),
				NHibernateUtil.Decimal,
				Projections.SubQuery(incomeSub),
				Projections.SubQuery(expenseSub)
			);

			var results = uow.Session
				.QueryOver(() => subdivisionAlias)
				.Where(() => subdivisionAlias.Id.IsIn(subdivisionIds)).SelectList(list => list
					.Select(() => subdivisionAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => subdivisionAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(projection).WithAlias(() => resultAlias.Balance)
				)
				.TransformUsing(Transformers.AliasToBean<BalanceNode>())
				.List<BalanceNode>();
			return results;
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

		public decimal CurrentRouteListCashReturn(IUnitOfWork uow, int routeListId)
		{
			decimal income = uow.Session.QueryOver<Income>()
								.Where(exp => exp.RouteListClosing.Id == routeListId)
								.Where(exp => exp.TypeOperation == IncomeType.Return)
								.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();

			return income;
		}

		public decimal CurrentRouteListTotalExpense(IUnitOfWork uow, int routeListId)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
								 .Where(exp => exp.RouteListClosing.Id == routeListId)
								 .Where(exp => exp.TypeOperation == ExpenseType.Expense)
								 .Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();

			return expense;
		}

		/// <summary>
		/// Возвращает сумму находящуюся в перемещении между кассами
		/// </summary>
		public decimal GetCashInTransferring(IUnitOfWork uow)
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

		public decimal GetIncomeSumByRouteListId(IUnitOfWork uow, int routeListId, IncomeType[] includedIncomeTypes = null, IncomeType[] excludedIncomeTypes = null)
		{
			var query = uow.Session.QueryOver<Income>()
				.Where(inc => inc.RouteListClosing.Id == routeListId)
				.Select(Projections.Sum<Income>(inc => inc.Money));

			if(includedIncomeTypes != null)
			{
				query.Where(inc => inc.IsIn(includedIncomeTypes));
			}

			if(excludedIncomeTypes != null)
			{
				query.Where(inc => !inc.IsIn(excludedIncomeTypes));
			}

			return query.SingleOrDefault<decimal>();
		}

		public decimal GetExpenseSumByRouteListId(IUnitOfWork uow, int routeListId, ExpenseType[] includedExpenseTypes = null, ExpenseType[] excludedExpenseTypes = null)
		{
			var query = uow.Session.QueryOver<Expense>()
				.Where(exp => exp.RouteListClosing.Id == routeListId)
				.Select(Projections.Sum<Expense>(exp => exp.Money));

			if(includedExpenseTypes != null)
			{
				query.Where(exp => exp.TypeOperation.IsIn(includedExpenseTypes));
			}

			if(excludedExpenseTypes != null)
			{
				query.Where(exp => !exp.TypeOperation.IsIn(excludedExpenseTypes));
			}

			return query.SingleOrDefault<decimal>();
		}
	}
}
