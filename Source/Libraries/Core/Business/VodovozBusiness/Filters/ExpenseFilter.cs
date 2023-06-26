using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using QS.DomainModel.Entity;
using QS.Tools;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Filters
{
	public class ExpenseFilter : PropertyChangedBase, IQueryFilter
	{
		private DateTime startDate;
		public DateTime StartDate {
			get => startDate;
			set => SetField(ref startDate, value, () => StartDate);
		}

		private DateTime endDate;
		public DateTime EndDate {
			get => endDate;
			set => SetField(ref endDate, value, () => EndDate);
		}


		private Employee employee;
		public Employee Employee {
			get => employee;
			set => SetField(ref employee, value, () => Employee);
		}

		private ExpenseCategory expenseCategory;
		public ExpenseCategory ExpenseCategory {
			get => expenseCategory;
			set => SetField(ref expenseCategory, value, () => ExpenseCategory);
		}

		public ExpenseFilter()
		{
			SetDefaults();
		}

		public void SetDefaults()
		{
			StartDate = DateTime.Today.AddDays(-7);
			EndDate = DateTime.Today;
		}

		public ICriterion GetFilter()
		{
			var dateCriterion = Projections.SqlFunction(
				   new SQLFunctionTemplate(
					   NHibernateUtil.Date,
					   "Date(?1)"
					  ),
				   NHibernateUtil.Date,
				   Projections.Property<Expense>(x => x.Date)
			);
			ICriterion result = Restrictions.And(Restrictions.Ge(dateCriterion, StartDate), Restrictions.Le(dateCriterion, EndDate));

			if(Employee != null) {
				result = Restrictions.And(result, Restrictions.Where<Expense>(x => x.Employee == Employee));
			}

			if(ExpenseCategory != null) {
				result = Restrictions.And(result, Restrictions.Where<Expense>(x => x.ExpenseCategoryId == ExpenseCategory.Id));
			}

			return result;
		}
	}
}
