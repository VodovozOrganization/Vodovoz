using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class HistoryChildSpecialMap : SubclassMap<HistoryChildSpecial>
	{
		public HistoryChildSpecialMap()
		{
			DiscriminatorValue("special");
			Map(x => x.Extra).Column("extra");
		}
	}
}
