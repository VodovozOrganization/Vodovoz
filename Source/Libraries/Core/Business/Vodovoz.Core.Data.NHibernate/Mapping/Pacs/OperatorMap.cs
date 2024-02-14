using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Pacs;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Pacs
{
	public class OperatorMap : ClassMap<Operator>
	{
		public OperatorMap()
		{
			Table("pacs_operators");

			Id(x => x.Id).Column("id")
                .GeneratedBy.Assigned();
			Map(x => x.PacsEnabled).Column("pacs_enabled");
			References(x => x.WorkShift).Column("workshift_id")
                .Not.LazyLoad()
                .Fetch.Join();
		}
	}
}
