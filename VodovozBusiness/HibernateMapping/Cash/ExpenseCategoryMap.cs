using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HibernateMapping
{
	public class ExpenseCategoryMap : ClassMap<ExpenseCategory>
	{
		public ExpenseCategoryMap ()
		{
			Table("cash_expense_category");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Name).Column ("name");

			References(x => x.Parent).Column("parent_id");
			HasMany (x => x.Childs).Inverse().Cascade.All ().LazyLoad ().KeyColumn ("parent_id");
		}
	}
}

