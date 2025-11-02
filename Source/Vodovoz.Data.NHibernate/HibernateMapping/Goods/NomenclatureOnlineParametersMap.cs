using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class NomenclatureOnlineParametersMap : ClassMap<NomenclatureOnlineParameters>
	{
		public NomenclatureOnlineParametersMap()
		{
			Table("nomenclatures_online_parameters");
			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.NomenclatureOnlineDiscount).Column("online_discount");
			Map(x => x.NomenclatureOnlineAvailability).Column("online_availability");
			Map(x => x.NomenclatureOnlineMarker).Column("online_marker");
			Map(x => x.Type).Column("type").Not.Update().Not.Insert().Access.ReadOnly();

			References(x => x.Nomenclature).Column("nomenclature_id");

			HasMany(x => x.NomenclatureOnlinePrices)
				.Cascade.AllDeleteOrphan().Inverse().KeyColumn("nomenclature_online_parameters_id");
		}
	}
}
