using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class FiscalInventPositionMap : ClassMap<FiscalInventPosition>
	{
		public FiscalInventPositionMap()
		{
			Table("edo_fiscal_invent_positions");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Name)
				.Column("name");

			Map(x => x.Quantity)
				.Column("quantity");

			Map(x => x.Price)
				.Column("price");

			Map(x => x.DiscountSum)
				.Column("discount_sum");

			Map(x => x.Vat)
				.Column("vat");

			References(x => x.EdoTaskItem)
				.Column("edo_task_item_id");

			References(x => x.RegulatoryDocument)
				.Column("regulatory_document_id");

			References(x => x.GroupCode)
				.Column("group_code_id");

			Map(x => x.IndustryRequisiteData)
				.Column("industry_requisite_data");

			HasManyToMany(x => x.OrderItems)
				.Table("edo_fiscal_invent_positions_order_items")
				.ParentKeyColumn("edo_fiscal_invent_position_id")
				.ChildKeyColumn("order_item_id")
				.LazyLoad();
		}
	}
}
