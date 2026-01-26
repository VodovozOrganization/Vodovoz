using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.Rent;

namespace Vodovoz.Core.Data.NHibernate.Goods.Rent
{
	public class FreeRentPackageMap : ClassMap<FreeRentPackageEntity>
	{
		public FreeRentPackageMap()
		{
			Table("free_rent_packages");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();
			
			Map(x => x.Name)
				.Column("name");

			Map(x => x.OnlineName)
				.Column("online_name");

			Map(x => x.MinWaterAmount)
				.Column("min_water_amount");

			Map(x => x.Deposit)
				.Column("deposit");

			Map(x => x.IsArchive)
				.Column("is_archive");

			References(x => x.EquipmentKind)
				.Column("equipment_kind_id");

			References(x => x.DepositService)
				.Column("deposit_service_id")
				.LazyLoad();

			HasMany(x => x.OnlineParameters)
				.KeyColumn("free_rent_package_id")
				.Inverse()
				.Cascade
				.AllDeleteOrphan();
		}
	}
}

