using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.FastDelivery;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.FastDelivery
{
	public class FastDeliveryNomenclatureDistributionHistoryMap : ClassMap<FastDeliveryNomenclatureDistributionHistory>
	{
		public FastDeliveryNomenclatureDistributionHistoryMap()
		{
			Table("fast_delivery_nomenclature_distribution_history");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Percent).Column("percent");

			References(x => x.Nomenclature).Column("nomenclature_id");
			References(x => x.FastDeliveryAvailabilityHistory).Column("fast_delivery_availability_history_id");
		}
	}
}
