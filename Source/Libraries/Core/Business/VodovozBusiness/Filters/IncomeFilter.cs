using System;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using QS.DomainModel.Entity;
using QS.Tools;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Filters
{
	public sealed class IncomeFilter : PropertyChangedBase, IQueryFilter
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

		private FinancialIncomeCategory _financialincomeCategory;
		public FinancialIncomeCategory FinancialIncomeCategory
		{
			get => _financialincomeCategory;
			set => SetField(ref _financialincomeCategory, value);
		}

		private FinancialExpenseCategory _financialExpenseCategory;
		public FinancialExpenseCategory FinancialExpenseCategory {
			get => _financialExpenseCategory;
			set => SetField(ref _financialExpenseCategory, value);
		}

		public IncomeFilter()
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
				   Projections.Property<Income>(x => x.Date)
			);
			ICriterion result = Restrictions.And(Restrictions.Ge(dateCriterion, StartDate), Restrictions.Le(dateCriterion, EndDate));

			if(Employee != null) {
				result = Restrictions.And(result, Restrictions.Where<Income>(x => x.Employee == Employee));
			}

			if(FinancialIncomeCategory != null) {
				result = Restrictions.And(result, Restrictions.Where<Income>(x => x.IncomeCategoryId == FinancialIncomeCategory.Id));
			}

			if(FinancialExpenseCategory != null) {
				result = Restrictions.And(result, Restrictions.Where<Income>(x => x.ExpenseCategoryId == FinancialExpenseCategory.Id));
			}

			return result;
		}
	}
}
