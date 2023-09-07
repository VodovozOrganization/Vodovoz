using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class UndeliveryProblemSourceMap : ClassMap<UndeliveryProblemSource>
	{
		public UndeliveryProblemSourceMap()
		{
			Table("undelivery_problem_sources");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.Name).Column("name");
		}
	}
}
