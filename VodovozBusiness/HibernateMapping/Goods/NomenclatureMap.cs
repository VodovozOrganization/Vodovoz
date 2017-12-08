using FluentNHibernate.Mapping;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;

namespace Vodovoz.HibernateMapping
{
	public class NomenclatureMap : ClassMap<Nomenclature>
	{
		public NomenclatureMap ()
		{
			Table ("nomenclature");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.Name)		 .Column ("name");
			Map (x => x.OfficialName).Column ("official_name");
			Map (x => x.Model)		 .Column ("model");
			Map (x => x.Weight)		 .Column ("weight");
			Map (x => x.Volume)  	 .Column ("volume");
			Map (x => x.VAT)		 .Column ("vat").CustomType<VATStringType> ();
			Map (x => x.DoNotReserve).Column ("reserve");
			Map (x => x.RentPriority).Column ("rent_priority");
			Map (x => x.IsSerial)	 .Column ("serial");
			Map (x => x.Category)	 .Column ("category").CustomType<NomenclatureCategoryStringType> ();
			Map (x => x.Code1c)		 .Column ("code_1c");
			Map (x => x.SumOfDamage) .Column ("sum_of_damage");
			Map (x => x.ShortName)	 .Column ("short_name");
			Map (x => x.Hide)   	 .Column ("hide");
			Map (x => x.NoDelivey)   .Column ("no_delivery"); 

			References (x => x.Unit)		   .Column ("unit_id").Not.LazyLoad ();
			References (x => x.Color)		   .Column ("color_id");
			References (x => x.Type)		   .Column ("type_id");
			References (x => x.Manufacturer)   .Column ("manufacturer_id");
			References (x => x.RouteListColumn).Column ("route_column_id");
			References (x => x.Warehouse)	   .Column ("warehouse_id");

			HasMany (x => x.NomenclaturePrice).Cascade.AllDeleteOrphan ().LazyLoad ().KeyColumn ("nomenclature_id");
		}
	}
}

