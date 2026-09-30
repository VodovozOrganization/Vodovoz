using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoTaskDeviationMap : ClassMap<EdoTaskDeviation>
	{
		public EdoTaskDeviationMap()
		{
			Table("edo_task_deviations");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			References(x => x.EdoTask)
				.Column("edo_task_id");

			References(x => x.EdoRequest)
				.Column("edo_request_id");

			References(x => x.DeviationSource)
				.Column("edo_deviation_source_id");

			Map(x => x.StageName)
				.Column("stage_name");

			Map(x => x.StageStartTime)
				.Column("stage_start_time");

			Map(x => x.Threshold)
				.Column("threshold")
				.CustomType<TimeAsTimeSpanType>();

			Map(x => x.Details)
				.Column("details");

			Map(x => x.State)
				.Column("state");

			Map(x => x.DetectedTime)
				.Column("detected_time");

			Map(x => x.ResolvedTime)
				.Column("resolved_time");

			Map(x => x.ResolveReason)
				.Column("resolve_reason");
		}
	}
}
