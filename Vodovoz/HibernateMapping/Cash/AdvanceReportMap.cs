using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HMap
{
	public class AdvanceReportMap : ClassMap<AdvanceReport>
	{
		public AdvanceReportMap ()
		{
			Table("cash_advance_report");
			Not.LazyLoad ();

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map (x => x.Date).Column ("date");
			References (x => x.Casher).Column ("casher_employee_id");
			References (x => x.Accountable).Column ("employee_id");
			References (x => x.ExpenseCategory).Column ("cash_expense_category_id");
			References (x => x.ChangeReturn).Column ("return_id");
			Map (x => x.Money).Column ("money");
			Map (x => x.Description).Column ("description");
		}
	}
}

