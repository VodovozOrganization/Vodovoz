using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoProblemCustomItemMap : ClassMap<EdoProblemCustomItem>
	{
		public EdoProblemCustomItemMap()
		{
			Table("edo_task_problem_custom_items");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			References(x => x.Problem)
				.Column("edo_task_problem_id");
		}
	}
}
