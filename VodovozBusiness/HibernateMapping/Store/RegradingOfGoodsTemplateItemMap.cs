using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Store;

namespace Vodovoz.HMap
{
	public class RegradingOfGoodsTemplateItemMap : ClassMap<RegradingOfGoodsTemplateItem>
	{
		public RegradingOfGoodsTemplateItemMap ()
		{
			Table ("store_regrading_of_goods_template_item");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			References (x => x.Template).Column ("store_regrading_of_goods_template_id").Not.Nullable ();
			References (x => x.NomenclatureOld).Column ("nomenclature_old_id").Not.Nullable ();
			References (x => x.NomenclatureNew).Column ("nomenclature_new_id").Not.Nullable ();
		}
	}
}