using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping
{
	public class PaidRentEquipmentMap : ClassMap<PaidRentEquipment>
	{
		public PaidRentEquipmentMap ()
		{
			Table ("paid_rent_equipment");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Deposit).Column ("deposit");
			Map (x => x.Count).Column("count");
			Map (x => x.Price).Column ("price");
			References (x => x.PaidRentPackage).Column ("paid_rent_package_id");
			References (x => x.Equipment).Column ("equipment_id");
		}
	}
}