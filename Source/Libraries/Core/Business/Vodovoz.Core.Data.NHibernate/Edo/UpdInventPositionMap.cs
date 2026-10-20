using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo.Upd;

namespace Vodovoz.Core.Data.NHibernate.Edo
{
	public class UpdInventPositionMap : ClassMap<UpdInventPosition>
	{
		public UpdInventPositionMap()
		{
			Table("edo_upd_document_invent_positions");

			Id(x => x.Id).GeneratedBy.Native();

			References(x => x.AssignedOrderItem)
				.Column("assigned_order_item_id");

			Map(x => x.Name)
				.Column("name");

			Map(x => x.IsService)
				.Column("is_service");

			Map(x => x.UnitName)
				.Column("unit_name");

			Map(x => x.OKEI)
				.Column("okei");

			Map(x => x.NomenclatureId)
				.Column("nomenclature_id");

			Map(x => x.Count)
				.Column("count");

			Map(x => x.Price)
				.Column("price");

			Map(x => x.IncludeVat)
				.Column("include_vat");

			Map(x => x.ValueAddedTax)
				.Column("value_added_tax");

			Map(x => x.DiscountMoney)
				.Column("discount_money");

			Map(x => x.SpecialCustomerNomenclatureId)
				.Column("special_customer_nomenclature_id");

			HasMany(x => x.Codes)
				.Cascade.AllDeleteOrphan()
				.KeyColumn("edo_upd_document_invent_position_id");
		}
	}
}
