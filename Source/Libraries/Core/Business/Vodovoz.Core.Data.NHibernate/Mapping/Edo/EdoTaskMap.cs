using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoTaskMap : ClassMap<EdoTask>
	{
		public EdoTaskMap()
		{
			Table("edo_tasks");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			DiscriminateSubClassesOnColumn("type");

			Map(x => x.TaskType)
				.Column("type")
				.ReadOnly();

			Map(x => x.CreationDate)
				.Column("creation_date")
				.ReadOnly();

			Map(x => x.Status)
				.Column("status");

			Map(x => x.StartTime)
				.Column("start_time");

			Map(x => x.EndTime)
				.Column("end_time");
		}
	}
}
