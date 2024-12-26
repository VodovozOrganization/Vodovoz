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

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.CreationTime)
				.Column("creation_time")
				.ReadOnly();

			Map(x => x.UpdateTime)
				.Column("updated_time")
				.ReadOnly();

			References(x => x.EdoTask)
				.Column("edo_task_id");

			Map(x => x.State)
				.Column("state");

			Map(x => x.ValidatorName)
				.Column("validator_name");

			HasManyToMany(x => x.TaskItems)
				.Table("edo_task_problem_items")
				.ParentKeyColumn("edo_task_problem_id")
				.ChildKeyColumn("customer_edo_task_item_id")
				.LazyLoad();
		}
	}
}
