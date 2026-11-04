using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class BulkChildSpecialMap : SubclassMap<BulkChildSpecial>
	{
		public BulkChildSpecialMap()
		{
			DiscriminatorValue("special");
			Map(x => x.Extra).Column("extra");
		}
	}
}
