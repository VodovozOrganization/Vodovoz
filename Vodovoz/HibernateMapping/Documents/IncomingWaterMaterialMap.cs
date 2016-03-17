using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HMap
{
	public class IncomingWaterMaterialMap : ClassMap<IncomingWaterMaterial>
	{
		public IncomingWaterMaterialMap ()
		{
			Table ("incoming_water_material");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.OneProductAmount).Column ("amount_by_product");
			Map (x => x.Amount).Column ("amount");
			References (x => x.Document).Column ("incoming_water_id").Not.Nullable ();
			References (x => x.Nomenclature).Column ("nomenclature_id").Not.Nullable ();
			References (x => x.ConsumptionMaterialOperation).Column ("consumption_material_id").Not.Nullable ().Cascade.All ();
		}
	}
}