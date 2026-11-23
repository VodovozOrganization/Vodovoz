using NHibernate.Linq;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Infrastructure.Persistance.Cash
{
	internal sealed partial class CashRepository
	{
		/// <inheritdoc/>
		public CashFlowReportDataNode GetCashFlowReportData(
			IUnitOfWork uow, DateTime startDate, DateTime endDate, bool useDdrDate, bool includeCenters)
		{
			var incomes = uow.Session.Query<Income>()
				.Where(x => x.Date >= startDate && x.Date <= endDate);

			var incomeTotals = incomes
				.Where(x => x.TypeOperation != IncomeType.Return && x.IncomeCategoryId != null)
				.GroupBy(x => x.IncomeCategoryId.Value)
				.Select(x => new { CategoryId = x.Key, Money = x.Sum(i => i.Money) })
				.ToDictionary(x => x.CategoryId, x => x.Money);

			var expenses = GetCashFlowReportExpenses(uow, startDate, endDate, useDdrDate, includeCenters);

			var returns = incomes
				.Where(x => x.TypeOperation == IncomeType.Return && x.ExpenseCategoryId != null)
				.Select(x => new CashFlowReportReturnNode
				{
					Id = x.Id,
					ExpenseCategoryId = x.ExpenseCategoryId.Value,
					Money = x.Money
				})
				.ToList();

			var directClosings = includeCenters && returns.Count > 0
				? GetCashFlowReportClosings(uow, startDate, endDate, byAdvanceReport: false)
				: Array.Empty<CashFlowReportAdvanceClosingNode>();
			var advanceReportClosings = includeCenters && returns.Count > 0
				? GetCashFlowReportClosings(uow, startDate, endDate, byAdvanceReport: true)
				: Array.Empty<CashFlowReportAdvanceClosingNode>();

			return new CashFlowReportDataNode
			{
				IncomeTotals = incomeTotals,
				Expenses = expenses,
				Returns = returns,
				DirectClosings = directClosings,
				AdvanceReportClosings = advanceReportClosings,
				SubdivisionNames = GetCashFlowReportSubdivisionNames(uow, expenses, directClosings.Concat(advanceReportClosings))
			};
		}

		private static IList<CashFlowReportExpenseNode> GetCashFlowReportExpenses(
			IUnitOfWork uow, DateTime startDate, DateTime endDate, bool useDdrDate, bool includeCenters)
		{
			Expense expense = null;
			Subdivision cfo = null;
			Subdivision cfz = null;
			CashFlowReportExpenseNode result = null;

			var query = uow.Session.QueryOver(() => expense)
				.Where(() => expense.ExpenseCategoryId != null);

			if(useDdrDate)
			{
				query.Where(() => expense.DdrDate >= startDate && expense.DdrDate <= endDate);
			}
			else
			{
				query.Where(() => expense.Date >= startDate && expense.Date <= endDate);
			}

			if(includeCenters)
			{
				query.Left.JoinAlias(() => expense.CfoSubdivision, () => cfo)
					.Left.JoinAlias(() => expense.CfzSubdivision, () => cfz);
			}

			return query
				.SelectList(list =>
				{
					list.SelectGroup(() => expense.ExpenseCategoryId).WithAlias(() => result.ExpenseCategoryId)
						.SelectSum(() => expense.Money).WithAlias(() => result.Money);

					if(includeCenters)
					{
						list.SelectGroup(() => cfo.Id).WithAlias(() => result.CfoSubdivisionId)
							.SelectGroup(() => cfz.Id).WithAlias(() => result.CfzSubdivisionId);
					}

					return list;
				})
				.TransformUsing(Transformers.AliasToBean<CashFlowReportExpenseNode>())
				.List<CashFlowReportExpenseNode>();
		}

		private static IList<CashFlowReportAdvanceClosingNode> GetCashFlowReportClosings(
			IUnitOfWork uow, DateTime startDate, DateTime endDate, bool byAdvanceReport)
		{
			AdvanceClosing closing = null;
			AdvanceReport advanceReport = null;
			Income cashReturn = null;
			Expense advance = null;
			Subdivision cfo = null;
			Subdivision cfz = null;
			CashFlowReportAdvanceClosingNode result = null;

			var query = uow.Session.QueryOver(() => closing)
				.JoinAlias(() => closing.AdvanceExpense, () => advance)
				.Left.JoinAlias(() => advance.CfoSubdivision, () => cfo)
				.Left.JoinAlias(() => advance.CfzSubdivision, () => cfz);

			if(byAdvanceReport)
			{
				query.JoinAlias(() => closing.AdvanceReport, () => advanceReport)
					.JoinAlias(() => advanceReport.ChangeReturn, () => cashReturn);
			}
			else
			{
				query.JoinAlias(() => closing.Income, () => cashReturn);
			}

			return query
				.Where(() => cashReturn.TypeOperation == IncomeType.Return && cashReturn.ExpenseCategoryId != null)
				.Where(() => cashReturn.Date >= startDate && cashReturn.Date <= endDate)
				.SelectList(list => list
					.Select(() => cashReturn.Id).WithAlias(() => result.ReturnId)
					.Select(() => cfo.Id).WithAlias(() => result.CfoSubdivisionId)
					.Select(() => cfz.Id).WithAlias(() => result.CfzSubdivisionId)
					.Select(() => closing.Money).WithAlias(() => result.Money))
				.TransformUsing(Transformers.AliasToBean<CashFlowReportAdvanceClosingNode>())
				.List<CashFlowReportAdvanceClosingNode>();
		}

		private static IReadOnlyDictionary<int, string> GetCashFlowReportSubdivisionNames(
			IUnitOfWork uow, IEnumerable<CashFlowReportExpenseNode> expenses, IEnumerable<CashFlowReportAdvanceClosingNode> closings)
		{
			var ids = expenses.Select(x => (x.CfoSubdivisionId, x.CfzSubdivisionId))
				.Concat(closings.Select(x => (x.CfoSubdivisionId, x.CfzSubdivisionId)))
				.SelectMany(centers => new[] { centers.CfoSubdivisionId, centers.CfzSubdivisionId })
				.Where(x => x.HasValue)
				.Select(x => x.Value)
				.Distinct()
				.ToArray();

			if(ids.Length == 0)
			{
				return new Dictionary<int, string>();
			}

			return uow.Session.Query<Subdivision>()
				.Where(x => ids.Contains(x.Id))
				.Select(x => new { x.Id, x.Name })
				.ToDictionary(x => x.Id, x => x.Name);
		}
	}
}
