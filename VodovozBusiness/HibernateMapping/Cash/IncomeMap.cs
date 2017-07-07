using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HibernateMapping
{
	public class IncomeMap : ClassMap<Income>
	{
		public IncomeMap ()
		{
			Table("cash_income");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.TypeOperation).Column ("type").CustomType<IncomeTypeStringType> ();
			Map (x => x.Date).Column ("date");
			References (x => x.Casher).Column ("casher_employee_id");
			References (x => x.Employee).Column ("employee_id");
			References (x => x.Customer).Column ("customer_id");
			References (x => x.IncomeCategory).Column ("cash_income_category_id");
			References (x => x.ExpenseCategory).Column ("cash_expense_category_id");
			References (x => x.RouteListClosing).Column("route_list_id");
			Map (x => x.Money).Column ("money");
			Map (x => x.Description).Column ("description");
		}
	}
}

