using FluentNHibernate.Mapping;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.HibernateMapping.WageCalculation
{
	public class WageDistrictMap : ClassMap<WageDistrict>
	{
		public WageDistrictMap()
		{
			Table("wage_districts");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
		}
	}
}
