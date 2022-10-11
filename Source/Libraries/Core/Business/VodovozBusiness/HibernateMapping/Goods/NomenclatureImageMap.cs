using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.HibernateMapping
{
	public class NomenclatureImageMap : ClassMap<NomenclatureImage>
	{
		public NomenclatureImageMap ()
		{
			Table ("nomenclature_images");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Size).Column ("size");
			Map (x => x.Image).Column ("image");

			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
		}
	}
}

