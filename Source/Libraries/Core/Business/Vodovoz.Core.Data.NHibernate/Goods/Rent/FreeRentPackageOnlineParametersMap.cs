using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.Rent;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods.Rent
{
	public class FreeRentPackageOnlineParametersMap : ClassMap<FreeRentPackageOnlineParametersEntity>
	{
		public FreeRentPackageOnlineParametersMap()
		{
			Table("free_rent_packages_online_parameters");
			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.PackageOnlineAvailability).Column("online_availability");
			Map(x => x.Type).Column("type").Not.Update().Not.Insert().Access.ReadOnly();

			References(x => x.FreeRentPackage).Column("free_rent_package_id");
		}
	}
}
