using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Edo
{
	public class OrderUpdOperationProductMap : ClassMap<OrderUpdOperationProduct>
	{
		public OrderUpdOperationProductMap()
		{
			Table("order_upd_operation_products");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.NomenclatureId).Column("nomenclature_id");
			Map(x => x.OKEI).Column("okei");
			Map(x => x.NomenclatureName).Column("nomenclature_name");
			Map(x => x.UnitCode).Column("unit_code");
			Map(x => x.MeasurementUnitName).Column("measurement_unit_name");
			Map(x => x.Count).Column("count");
			Map(x => x.ItemPrice).Column("item_price");
			Map(x => x.IncludeVat).Column("include_vat");
			Map(x => x.Vat).Column("vat");
			Map(x => x.ItemDiscount).Column("item_discount");
			Map(x => x.ItemDiscountMoney).Column("item_discount_money");
			Map(x => x.IsService).Column("is_service");
		}
	}
}
