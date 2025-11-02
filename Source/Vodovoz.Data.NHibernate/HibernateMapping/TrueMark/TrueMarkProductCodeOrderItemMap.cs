using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;

namespace Vodovoz.Data.NHibernate.HibernateMapping.TrueMark
{
	public class TrueMarkProductCodeOrderItemMap : ClassMap<TrueMarkProductCodeOrderItem>
	{
		public TrueMarkProductCodeOrderItemMap()
		{
			Table("true_mark_product_codes_order_items");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.TrueMarkProductCodeId).Column("true_mark_product_code_id");
			Map(x => x.OrderItemId).Column("order_item_id");
		}
	}
}
