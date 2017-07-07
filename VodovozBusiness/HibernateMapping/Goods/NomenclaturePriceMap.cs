using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.HibernateMapping
{
	public class NomenclaturePriceMap : ClassMap<NomenclaturePrice>
	{
		public NomenclaturePriceMap ()
		{
			Table ("nomenclature_price");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.MinCount).Column ("min_count");
			Map (x => x.Price).Column ("price");
		}
	}
}

