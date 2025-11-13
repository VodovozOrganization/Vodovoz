using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoTaskProblemMap : ClassMap<EdoTaskProblem>
	{
		public EdoTaskProblemMap()
		{
			Table("edo_task_problems");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.CreationTime)
				.Column("creation_time")
				.ReadOnly();

			Map(x => x.UpdateTime)
				.Column("updated_time")
				.ReadOnly();

			Map(x => x.Type)
				.Column("type")
				.ReadOnly();

			References(x => x.EdoTask)
				.Column("edo_task_id");

			Map(x => x.SourceName)
				.Column("source_name");

			Map(x => x.State)
				.Column("state");

			HasManyToMany(x => x.TaskItems)
				.Table("edo_task_problem_items")
				.ParentKeyColumn("edo_task_problem_id")
				.ChildKeyColumn("order_edo_task_item_id")
				.LazyLoad();

			HasMany(x => x.CustomItems)
				.Table("edo_task_problem_custom_items")
				.KeyColumn("edo_task_problem_id")
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.LazyLoad();
		}
	}
}
