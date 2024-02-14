using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Pacs
{
	public class CallEventMap : ClassMap<CallEvent>
	{
		public CallEventMap()
		{
			Table("pacs_call_events");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.CreationTime).Column("creation_time").ReadOnly();
			Map(x => x.EventTime).Column("event_time");
			Map(x => x.CallId).Column("call_id");
			Map(x => x.CallSequence).Column("call_sequence");
			Map(x => x.CallState).Column("call_state");
			Map(x => x.DisconnectReason).Column("disconnect_reason");
			Map(x => x.FromNumber).Column("from_number");
			Map(x => x.FromExtension).Column("from_extension");
			Map(x => x.TakenFromCallId).Column("taken_from_call_id");
			Map(x => x.ToNumber).Column("to_number");
			Map(x => x.ToExtension).Column("to_extension");
		}
	}
}
