using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HMap
{
	public class ExpenseMap : ClassMap<Expense>
	{
		public ExpenseMap ()
		{
			Table("cash_expense");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.TypeOperation).Column ("type").CustomType<ExpenseTypeStringType> ();
			Map (x => x.Date).Column ("date");
			References (x => x.Casher).Column ("casher_employee_id");
			References (x => x.Employee).Column ("employee_id");
			References (x => x.ExpenseCategory).Column ("cash_expense_category_id");
			Map (x => x.Money).Column ("money");
			Map (x => x.AdvanceClosed).Column ("advance_closed");
			Map (x => x.Description).Column ("description");

			HasMany (x => x.AdvanceCloseItems).Cascade.AllDeleteOrphan ().Inverse ().LazyLoad ().KeyColumn ("expense_id");
		}
	}
}

