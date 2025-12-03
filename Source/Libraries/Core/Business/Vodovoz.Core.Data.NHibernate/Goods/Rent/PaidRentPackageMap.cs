using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.Rent;

namespace Vodovoz.Core.Data.NHibernate.Goods.Rent
{
	public class PaidRentPackageMap : ClassMap<PaidRentPackageEntity>
	{
		public PaidRentPackageMap()
		{
			Table("paid_rent_packages");

			Id(x => x.Id).Column("id")
				.GeneratedBy.Native();
			
			Map(x => x.Name)
				.Column("name");

			Map(x => x.PriceDaily)
				.Column("price_daily");

			Map(x => x.PriceMonthly)
				.Column("price_monthly");

			Map(x => x.Deposit)
				.Column("deposit");
			
			References(x => x.EquipmentKind)
				.Column("equipment_kind_id");

			References(x => x.RentServiceDaily)
				.Column("rent_service_daily_id")
				.LazyLoad();

			References(x => x.RentServiceMonthly)
				.Column("rent_service_monthly_id")
				.LazyLoad();

			References(x => x.DepositService)
				.Column("deposit_service_id")
				.LazyLoad();
		}
	}
}

