using System;
using NHibernate.Criterion;
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
			ICriterion result = Restrictions.Where<Expense>(x => x.Date >= StartDate && x.Date <= EndDate);

			if(Employee != null) {
				result = Restrictions.And(result, Restrictions.Where<Expense>(x => x.Employee == Employee));
			}

			if(ExpenseCategory != null) {
				result = Restrictions.And(result, Restrictions.Where<Expense>(x => x.ExpenseCategory == ExpenseCategory));
			}

			return result;
		}
	}
}
