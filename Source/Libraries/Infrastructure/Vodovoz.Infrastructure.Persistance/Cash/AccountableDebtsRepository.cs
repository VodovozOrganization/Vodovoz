using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Cash;

namespace Vodovoz.Infrastructure.Persistance.Cash
{
	internal sealed class AccountableDebtsRepository : IAccountableDebtsRepository
	{
		public decimal EmployeeDebt(IUnitOfWork uow, Employee accountable)
		{
			decimal received = uow.Session.QueryOver<Expense>()
				.Where(e => e.Employee == accountable && e.TypeOperation == ExpenseType.Advance)
				.Select(Projections.Sum<Expense>(o => o.Money)).SingleOrDefault<decimal>();

			decimal returned = uow.Session.QueryOver<Income>()
				.Where(i => i.Employee == accountable && i.TypeOperation == IncomeType.Return)
				.Select(Projections.Sum<Income>(o => o.Money)).SingleOrDefault<decimal>();

			decimal reported = uow.Session.QueryOver<AdvanceReport>()
				.Where(a => a.Accountable == accountable)
				.Select(Projections.Sum<AdvanceReport>(o => o.Money)).SingleOrDefault<decimal>();

			return received - returned - reported;
		}

		public IEnumerable<Expense> GetUnclosedAdvances(
			IUnitOfWork unitOfWork,
			Employee accountableEmployee,
			int? expenseCategoryId,
			int? organisationId)
		{
			Expense expenseAlias = null;

			var queryOver = unitOfWork.Query(() => expenseAlias)
				.Where(() => expenseAlias.TypeOperation == ExpenseType.Advance)
				.And(() => expenseAlias.Employee == accountableEmployee)
				.And(() => expenseAlias.AdvanceClosed == false)
				.And(() => expenseCategoryId == null || expenseAlias.ExpenseCategoryId == expenseCategoryId)
				.And(() => organisationId == null || expenseAlias.Organisation == null || expenseAlias.Organisation.Id == organisationId)
				.List();

			return queryOver.AsEnumerable();
		}
	}
}
