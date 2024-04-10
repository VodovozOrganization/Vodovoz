using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Pacs
{
	public class SubCallMap : ClassMap<SubCall>
	{
		public SubCallMap()
		{
			Table("pacs_sub_calls");

			Id(x => x.CallId).Column("call_id").GeneratedBy.Assigned();
			Map(x => x.CreationTime).Column("creation_time").ReadOnly();

			Map(x => x.EntryId).Column("entry_id");

			Map(x => x.State).Column("state");
			Map(x => x.WasConnected).Column("was_connected");
			Map(x => x.LastSeq).Column("last_seq");

			Map(x => x.StartTime).Column("start_time");
			Map(x => x.EndTime).Column("end_time");
			Map(x => x.FromNumber).Column("from_number");
			Map(x => x.FromExtension).Column("from_extension");
			Map(x => x.ToNumber).Column("to_number");
			Map(x => x.ToExtension).Column("to_extension");
			Map(x => x.ToLineNumber).Column("to_line_number");
			Map(x => x.ToAcdGroup).Column("to_acd_group");
			Map(x => x.TakenFromCallId).Column("taken_from_call_id");
			Map(x => x.DisconnectReason).Column("disconnect_reason");
		}
	}
}
