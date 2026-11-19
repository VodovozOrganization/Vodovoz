using FluentNHibernate.Mapping;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests.Domain
{
	public class SecondaryTableChildMap : ClassMap<SecondaryTableChild>
	{
		public SecondaryTableChildMap()
		{
			Table("bulk_secondary_children");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Join("bulk_secondary_ext", join =>
			{
				join.KeyColumn("child_id");
				join.Map(x => x.Extra).Column("extra");
			});
		}
	}
}
