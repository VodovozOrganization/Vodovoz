using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Pacs
{
	public class OperatorStateMap : ClassMap<OperatorState>
	{
		public OperatorStateMap()
		{
			Table("pacs_operator_states");

			Id(x => x.Id).Column("id")
                .GeneratedBy.Native();
			Map(x => x.OperatorId).Column("operator_id");
			References(x => x.Session).Column("session_id")
				.Cascade.All()
                .Not.LazyLoad()
                .Fetch.Join();
			References(x => x.WorkShift).Column("workshift_id")
				.Cascade.All()
				.Not.LazyLoad()
				.Fetch.Join();
			Map(x => x.Started).Column("started");
			Map(x => x.Ended).Column("ended");
			Map(x => x.Trigger).Column("operator_trigger");
			Map(x => x.State).Column("state");
			Map(x => x.PhoneNumber).Column("phone_number");
			Map(x => x.CallId).Column("call_id");
			Map(x => x.DisconnectionType).Column("disconnection_type");
			Map(x => x.BreakType).Column("break_type");
			Map(x => x.BreakChangedBy).Column("break_changed_by");
			Map(x => x.BreakChangedByAdminId).Column("break_changed_by_admin_id");
			Map(x => x.BreakAdminReason).Column("break_admin_reason");
		}
	}
}
