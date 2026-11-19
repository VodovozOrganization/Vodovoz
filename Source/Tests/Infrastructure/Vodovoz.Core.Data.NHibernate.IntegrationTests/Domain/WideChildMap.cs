using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class WideChildMap : ClassMap<WideChild>
	{
		public WideChildMap()
		{
			Table("bulk_wide_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.C1).Column("c1");
			Map(x => x.C2).Column("c2");
			Map(x => x.C3).Column("c3");
			Map(x => x.C4).Column("c4");
			Map(x => x.C5).Column("c5");
			Map(x => x.C6).Column("c6");
			Map(x => x.C7).Column("c7");
			Map(x => x.C8).Column("c8");
			Map(x => x.C9).Column("c9");
			Map(x => x.C10).Column("c10");
		}
	}
}
