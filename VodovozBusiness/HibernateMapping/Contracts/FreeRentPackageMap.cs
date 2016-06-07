using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HMap
{
	public class FreeRentPackageMap : ClassMap<FreeRentPackage>
	{
		public FreeRentPackageMap ()
		{
			Table ("free_rent_packages");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
			Map (x => x.MinWaterAmount).Column ("min_water_amount");
			Map (x => x.Deposit).Column ("deposit");
			References (x => x.EquipmentType).Column ("equipment_type_id");
			References (x => x.DepositService).Column ("deposit_service_id").LazyLoad ();
		}
	}
}

