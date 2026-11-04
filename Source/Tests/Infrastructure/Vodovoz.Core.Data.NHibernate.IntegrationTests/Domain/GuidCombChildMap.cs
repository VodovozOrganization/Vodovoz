using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class GuidCombChildMap : ClassMap<GuidCombChild>
	{
		public GuidCombChildMap()
		{
			Table("bulk_guid_comb_children");
			Id(x => x.Id).Column("id").GeneratedBy.GuidComb();
			Map(x => x.Name).Column("name");
		}
	}
}
