using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.CashTransfer;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Infrastructure.Persistance.Cash
{
	internal sealed class CashRepository : ICashRepository
	{
		public decimal GetIncomePaidSumForOrder(IUnitOfWork uow, int orderId, int? excludedIncomeDoc = null)
		{
			var query = uow.Session.QueryOver<Income>().Where(x => x.Order.Id == orderId);
			if(excludedIncomeDoc != null)
			{
				query.Where(x => x.Id != excludedIncomeDoc);
			}
			return query.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();
		}

		public decimal GetExpenseReturnSumForOrder(IUnitOfWork uow, int orderId, int? excludedExpenseDoc = null)
		{
			var query = uow.Session.QueryOver<Expense>().Where(x => x.Order.Id == orderId);
			if(excludedExpenseDoc != null)
			{
				query.Where(x => x.Id != excludedExpenseDoc);
			}
			return query.Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();
		}

		public bool OrderHasIncome(IUnitOfWork uow, int orderId)
		{
			var query = uow.Session.QueryOver<Income>()
						   .Where(x => x.Order.Id == orderId)
						   .List();

			return query.Any();
		}

		public decimal CurrentCash(IUnitOfWork uow)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
				.Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();

			decimal income = uow.Session.QueryOver<Income>()
				.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();

			return income - expense;
		}

		public decimal CurrentCashForOrganization(IUnitOfWork uow, Organization organization)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
				.Where(x => x.Organisation == organization)
				.Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();

			decimal income = uow.Session.QueryOver<Income>()
				.Where(x => x.Organisation == organization)
				.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();

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

		public IEnumerable<EmployeeBalanceNode> CurrentCashForGivenSubdivisions(IUnitOfWork uow, int[] subdivisionsIds)
		{
			var incomeEmployees =
				from income in uow.Session.Query<Income>()
				where subdivisionsIds.Contains(income.RelatedToSubdivision.Id)
				group income by new
				{
					income.RelatedToSubdivision,
					income.Casher,
					income.Date.Date
				} into g				
				select new EmployeeBalanceNode
				{
					SubdivisionId = g.Key.RelatedToSubdivision.Id,
					SubdivisionName = g.Key.RelatedToSubdivision.Name,
					Balance = g.Sum(b => b.Money),
					Cashier = g.Key.Casher,
					Date = g.Key.Date
				};

			var expenseEmployees =			
				from expense in uow.Session.Query<Expense>()
				where subdivisionsIds.Contains(expense.RelatedToSubdivision.Id)
				group expense by new
				{
					expense.RelatedToSubdivision,
					expense.Casher,
					expense.Date.Date
				} into g
				select new EmployeeBalanceNode
				{
					SubdivisionId = g.Key.RelatedToSubdivision.Id,
					SubdivisionName = g.Key.RelatedToSubdivision.Name,
					Balance = - g.Sum(b => b.Money),
					Cashier = g.Key.Casher,
					Date = g.Key.Date
				};

			var result = incomeEmployees.ToArray()
				.Concat(expenseEmployees.ToArray());

			return result;
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

		public IEnumerable<(int SubdivisionId, decimal Income, decimal Expense)> CashForSubdivisionsByDate(
			IUnitOfWork uow, IEnumerable<int> subdivisionsIds, DateTime date)
		{
			Subdivision subdivisionAlias = null;
			(int SubdivisionId, decimal Income, decimal Expense) resultAlias = default;

			var expenseProjection = QueryOver.Of<Expense>()
				.Where(e => e.RelatedToSubdivision.Id == subdivisionAlias.Id)
				.And(e => e.Date <= date)
				.Select(Projections.Sum<Expense>(e => e.Money));
			
			var incomeProjection = QueryOver.Of<Income>()
				.Where(i => i.RelatedToSubdivision.Id == subdivisionAlias.Id)
				.And(i => i.Date <= date)
				.Select(Projections.Sum<Income>(i => i.Money));
			
			var result = uow.Session.QueryOver(() => subdivisionAlias)
				.WhereRestrictionOn(s => s.Id).IsInG(subdivisionsIds)
				.SelectList(list => list
					.SelectGroup(s => s.Id).WithAlias(() => resultAlias.SubdivisionId)
					.SelectSubQuery(incomeProjection).WithAlias(() => resultAlias.Income)
					.SelectSubQuery(expenseProjection).WithAlias(() => resultAlias.Expense))
				.TransformUsing(Transformers.AliasToBean<(int SubdivisionId, decimal Income, decimal Expense)>())
				.List<(int SubdivisionId, decimal Income, decimal Expense)>();

			return result;
		}

		public decimal GetRouteListBalanceExceptAccountableCash(IUnitOfWork uow, int routeListId)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
								 .Where(exp => exp.RouteListClosing.Id == routeListId)
								 .Where(exp => exp.TypeOperation != ExpenseType.Advance)
								 .Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();

			decimal income = uow.Session.QueryOver<Income>()
								.Where(exp => exp.RouteListClosing.Id == routeListId)
								.Where(exp => exp.TypeOperation != IncomeType.Return)
								.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();

			return income - expense;
		}

		public decimal GetRouteListCashReturnSum(IUnitOfWork uow, int routeListId)
		{
			decimal income = uow.Session.QueryOver<Income>()
								.Where(exp => exp.RouteListClosing.Id == routeListId)
								.Where(exp => exp.TypeOperation == IncomeType.Return)
								.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();

			return income;
		}

		public decimal GetRouteListAdvancsReportsSum(IUnitOfWork uow, int routeListId)
		{
			decimal advanceReports = uow.Session.QueryOver<AdvanceReport>()
				.Where(exp => exp.RouteList.Id == routeListId)
				.Select(Projections.Sum<AdvanceReport>(o => o.Money)).SingleOrDefault<decimal>();

			return advanceReports;
		}

		public decimal GetRouteListCashExpensesSum(IUnitOfWork uow, int routeListId)
		{
			decimal expense = uow.Session.QueryOver<Expense>()
				.Where(exp => exp.RouteListClosing.Id == routeListId)
				.Where(exp => exp.TypeOperation == ExpenseType.Advance)
				.Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();

			return expense;
		}

		/// <summary>
		/// Возвращает сумму находящуюся в перемещении между кассами
		/// </summary>
		public decimal GetCashInTransferring(IUnitOfWork uow, DateTime? startDate = null, DateTime? endDate = null)
		{
			CashTransferOperation cashTransferOperationAlias = null;
			CashTransferDocumentBase cashTransferDocumentAlias = null;
			var result = uow.Session.QueryOver<CashTransferDocumentBase>(() => cashTransferDocumentAlias)
				.Left.JoinAlias(() => cashTransferDocumentAlias.CashTransferOperation, () => cashTransferOperationAlias)
				.Where(() => cashTransferDocumentAlias.Status != CashTransferDocumentStatuses.Received)
				.Where(() => cashTransferDocumentAlias.Status != CashTransferDocumentStatuses.New)
				.Where(() => cashTransferOperationAlias.ReceiveTime == null);

			if(startDate != null && endDate != null)
			{
				result.Where(c => c.SendTime >= startDate && c.SendTime <= endDate);
			}
			
			return result
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

		public IList<int> GetCashTransferDocumentsIdsByExpenseId(IUnitOfWork uow, int expenseId)
		{
			var cashTransferDocumentsHavingExpense = uow.Session.Query<ExpenseCashTransferedItem>()
				.Where(ti => ti.Expense.Id == expenseId)
				.Select(ti => ti.Document.Id)
				.ToList();

			return cashTransferDocumentsHavingExpense;
		}

		public IList<int> GetCashDistributionDocumentsIdsByFuelDocumentId(IUnitOfWork uow, int fuelDocumentId)
		{
			var cashDistributionDocuments = uow.Session.Query<FuelExpenseCashDistributionDocument>()
				.Where(d => d.FuelDocument.Id == fuelDocumentId)
				.Select(d => d.Id)
				.ToList();

			return cashDistributionDocuments;
		}

		public void DeleteFuelExpenseCashDistributionDocuments(IUnitOfWork uow, IEnumerable<int> documentIds)
		{
			if(!documentIds.Any())
			{
				return;
			}

			foreach(var cashDistributionDocumentsId in documentIds)
			{
				var document = uow.GetById<FuelExpenseCashDistributionDocument>(cashDistributionDocumentsId);

				if(document != null)
				{
					uow.Delete(document);
				}
			}
		}
	}
}
