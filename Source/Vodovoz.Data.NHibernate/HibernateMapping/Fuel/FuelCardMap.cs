using FluentNHibernate.Mapping;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Fuel
{
	public class FuelCardMap : ClassMap<FuelCard>
	{
		public FuelCardMap()
		{
			Table("fuel_cards");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CardId).Column("card_id");
			Map(x => x.CardNumber).Column("card_number");
			Map(x => x.IsArchived).Column("is_archived");
		}
	}
}
