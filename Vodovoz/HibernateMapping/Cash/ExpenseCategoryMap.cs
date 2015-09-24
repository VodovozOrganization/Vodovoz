using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HMap
{
	public class ExpenseCategoryMap : ClassMap<ExpenseCategory>
	{
		public ExpenseCategoryMap ()
		{
			Table("cash_expense_category");
			Not.LazyLoad ();

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Name).Column ("name");
		}
	}
}

