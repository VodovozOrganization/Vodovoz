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
			Not.LazyLoad ();

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			References (x => x.AdvanceExpense).Column ("expense_id").Not.Nullable ();
			References (x => x.AdvanceReport).Column ("advance_report_id");
			References (x => x.Income).Column ("income_id");
		}
	}
}

