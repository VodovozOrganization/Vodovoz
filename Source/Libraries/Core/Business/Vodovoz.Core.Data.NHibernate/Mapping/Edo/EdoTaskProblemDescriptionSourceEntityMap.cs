using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoTaskProblemDescriptionSourceEntityMap : ClassMap<EdoTaskProblemDescriptionSourceEntity>
	{
		public EdoTaskProblemDescriptionSourceEntityMap()
		{
			Table("edo_task_problem_description_sources");

			HibernateMapping.DefaultAccess.Property();

			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Name)
				.Column("name")
				.GeneratedBy.Assigned();

			Map(x => x.Type)
				.Column("type")
				.ReadOnly();

			Map(x => x.Importance)
				.Column("importance");

			Map(x => x.Description)
				.Column("description");

			Map(x => x.Recommendation)
				.Column("recommendation");
		}
	}
}
