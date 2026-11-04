using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class HiLoChildMap : ClassMap<HiLoChild>
	{
		public HiLoChildMap()
		{
			Table("bulk_hilo_children");
			Id(x => x.Id).Column("id").GeneratedBy.HiLo("bulk_hilo_keys", "next_hi", "100");
			Map(x => x.Name).Column("name");
		}
	}
}
