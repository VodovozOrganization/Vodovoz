using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Data.NHibernate.Goods
{
	public class NomenclaturePurchasePriceMap : ClassMap<NomenclaturePurchasePrice>
	{
		public NomenclaturePurchasePriceMap()
		{
			Table("nomenclature_purchase_prices");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.PurchasePrice).Column("purchase_price");

			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
		}
	}
}
