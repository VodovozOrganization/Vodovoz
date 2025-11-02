using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoTaskItemMap : ClassMap<EdoTaskItem>
	{
		public EdoTaskItemMap()
		{
			Table("edo_order_task_items");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			References(x => x.CustomerEdoTask)
				.Column("order_edo_task_id");

			References(x => x.ProductCode)
				.Column("product_code_id");
		}
	}
}
