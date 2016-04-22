using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HMap
{
	public class EquipmentMap : ClassMap<Equipment>
	{
		public EquipmentMap ()
		{
			Table ("equipment");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.LastServiceDate).Column ("last_service_date");
			Map (x => x.WarrantyEndDate).Column ("warranty_end_date");
			Map (x => x.OnDuty).Column ("on_duty");
			Map (x => x.Comment).Column ("comment");

			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.AssignedToClient).Column("assigned_to_client_id");
		}
	}
}

