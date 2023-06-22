﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HibernateMapping
{
	public class AdvanceReportMap : ClassMap<AdvanceReport>
	{
		public AdvanceReportMap ()
		{
			Table("cash_advance_report");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map (x => x.Date).Column ("date");
			Map (x => x.Money).Column ("money");
			Map (x => x.Description).Column ("description");

			References (x => x.Casher).Column ("casher_employee_id");
			References (x => x.Accountable).Column ("employee_id");
			References (x => x.ExpenseCategory).Column ("cash_expense_category_id");
			References (x => x.ChangeReturn).Column ("return_id");
			References(x => x.RelatedToSubdivision).Column("related_to_subdivision_id");
			References(x => x.Organisation).Column("organisation_id");
			References(x => x.RouteList).Column("route_list_id");
		}
	}
}

