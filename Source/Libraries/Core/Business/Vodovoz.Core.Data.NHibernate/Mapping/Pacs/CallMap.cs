using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Pacs
{
	public class CallMap : ClassMap<Call>
	{
		public CallMap()
		{
			Table("pacs_calls");

			Id(x => x.EntryId).Column("entry_id").GeneratedBy.Assigned();
			Map(x => x.CreationTime).Column("creation_time").ReadOnly();
			Map(x => x.CallId).Column("call_id");
			Map(x => x.StartTime).Column("start_time");
			Map(x => x.EndTime).Column("end_time");
			Map(x => x.FromNumber).Column("from_number");
			Map(x => x.FromExtension).Column("from_extension");
			Map(x => x.ToNumber).Column("to_number");
			Map(x => x.ToExtension).Column("to_extension");
			Map(x => x.ToLineNumber).Column("to_line_number");
			Map(x => x.DisconnectReason).Column("disconnect_reason");
			Map(x => x.CallDirection).Column("call_direction");
			Map(x => x.EntryResult).Column("entry_result");
			Map(x => x.Status).Column("status");

			HasMany(x => x.SubCalls).KeyColumn("entry_id")
				.Cascade.All()
				.Fetch.Join()
				.Not.LazyLoad()
				.Inverse();
		}
	}
}
