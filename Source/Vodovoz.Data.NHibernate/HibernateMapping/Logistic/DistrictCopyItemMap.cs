using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class DistrictCopyItemMap : ClassMap<DistrictCopyItem>
	{
		public DistrictCopyItemMap()
		{
			Table("district_copy_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.District).Column("district_id");
			References(x => x.CopiedToDistrict).Column("copied_to_district_id");
		}
	}
}
