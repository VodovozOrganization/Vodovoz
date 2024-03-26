using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class ExternalCreatingDeliveryPointMap : ClassMap<ExternalCreatingDeliveryPoint>
	{
		public ExternalCreatingDeliveryPointMap()
		{
			Table("external_creating_delivery_points");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.UniqueKey).Column("unique_key");
			Map(x => x.Source).Column("source");
			Map(x => x.CreatingDate).Column("creating_date");
		}
	}
}
