using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OnlineRentPackageMap : ClassMap<OnlineRentPackage>
	{
		public OnlineRentPackageMap()
		{
			Table("online_rent_packages");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.RentPackageId).Column("rent_package_id");
			Map(x => x.Price).Column("price");
			Map(x => x.Count).Column("count");
		}
	}
}
