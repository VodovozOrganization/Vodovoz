using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Pacs
{
	public class OperatorMap : ClassMap<Operator>
	{
		public OperatorMap()
		{
			Table("pacs_operators");

			Id(x => x.Id).Column("id")
                .GeneratedBy.Assigned();
			References(x => x.State).Column("state_id")
                .Not.LazyLoad()
                .Fetch.Join();
		}
	}

	public class OperatorStateMap : ClassMap<OperatorState>
	{
		public OperatorStateMap()
		{
			Table("pacs_operator_states");

			Id(x => x.Id).Column("id")
                .GeneratedBy.Native();
			Map(x => x.OperatorId).Column("operator_id");
			References(x => x.Session).Column("session_id")
                .Not.LazyLoad()
                .Fetch.Join();
			Map(x => x.Started).Column("started");
			Map(x => x.Ended).Column("ended");
			Map(x => x.Trigger).Column("operator_trigger");
			Map(x => x.State).Column("state");
			Map(x => x.PhoneNumber).Column("phone_number");
			Map(x => x.CallId).Column("call_id");
			Map(x => x.DisconnectionType).Column("disconnection_type");
		}
	}

	public class InnternalPhoneMap : ClassMap<InternalPhone>
	{
		public InnternalPhoneMap()
		{
			Table("internal_phones");

			Id(x => x.PhoneNumber).Column("phone_number")
                .GeneratedBy.Assigned();
			Map(x => x.Description).Column("description");
		}
	}

	public class SessionMap : ClassMap<Session>
	{
		public SessionMap()
		{
			Table("pacs_sessions");

			DiscriminateSubClassesOnColumn("type");
			Id(x => x.Id).Column("id")
                .GeneratedBy.Assigned();
			Map(x => x.Started).Column("started");
			Map(x => x.Ended).Column("ended");
		}
	}

	public class OperatorSessionMap : SubclassMap<OperatorSession>
	{
		public OperatorSessionMap()
		{
			DiscriminatorValue("Operator");
			Map(x => x.OperatorId).Column("operator_id");
		}
	}

	public class AdministratorSessionMap : SubclassMap<AdministratorSession>
	{
		public AdministratorSessionMap()
		{
			DiscriminatorValue("Administrator");
			Map(x => x.AdministratorId).Column("administrator_id");
		}
	}
}
