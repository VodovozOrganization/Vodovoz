using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OnlineFreeRentPackageMap : ClassMap<OnlineFreeRentPackage>
	{
		public OnlineFreeRentPackageMap()
		{
			Table("online_free_rent_packages");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.FreeRentPackageId).Column("first_free_rent_package_id");
			Map(x => x.Price).Column("price");
			Map(x => x.Count).Column("count");
			Map(x => x.FreeRentPackagePriceFromProgram).Column("free_rent_package_price_from_program");
			
			References(x => x.FreeRentPackage).Column("free_rent_package_id");
			References(x => x.OnlineOrder).Column("online_order_id");
		}
	}
}
