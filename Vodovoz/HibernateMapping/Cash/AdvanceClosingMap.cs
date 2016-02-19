using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HMap
{
	public class AdvanceClosingMap : ClassMap<AdvanceClosing>
	{
		public AdvanceClosingMap ()
		{
			Table("cash_advance_closing");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map (x => x.Money).Column ("money");
			References (x => x.AdvanceExpense).Column ("expense_id").Not.Nullable ();
			References (x => x.AdvanceReport).Column ("advance_report_id").Not.LazyLoad();
			References (x => x.Income).Column ("income_id").Not.LazyLoad();
		}
	}
}

