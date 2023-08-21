﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping
{
	public class FreeRentEquipmentMap : ClassMap<FreeRentEquipment>
	{
		public FreeRentEquipmentMap ()
		{
			Table ("free_rent_equipment");

			Id  (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Deposit).Column ("deposit");
			Map (x => x.Count).Column("count");
			Map (x => x.WaterAmount).Column ("water_amount");
			References (x => x.FreeRentPackage).Column ("free_rent_package_id");
			References (x => x.Equipment).Column ("equipment_id");
			References (x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}