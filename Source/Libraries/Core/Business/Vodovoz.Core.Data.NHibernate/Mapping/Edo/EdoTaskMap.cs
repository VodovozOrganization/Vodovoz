using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoTaskMap : ClassMap<EdoTask>
	{
		public EdoTaskMap()
		{
			Table("edo_tasks");

			OptimisticLock.Version();
			Version(x => x.Version)
				.Column("version");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.CreationTime)
				.Column("creation_time")
				.ReadOnly();

			Map(x => x.Status)
				.Column("status");

			Map(x => x.StartTime)
				.Column("start_time");

			Map(x => x.EndTime)
				.Column("end_time");

			Map(x => x.CancellationReason)
				.Column("cancellation_reason");

			HasMany(x => x.Problems)
				.KeyColumn("edo_task_id")
				.Cascade.AllDeleteOrphan()
				.Inverse();
		}
	}
}
