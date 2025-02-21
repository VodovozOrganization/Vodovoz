﻿using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class FiscalInventPositionMap : ClassMap<FiscalInventPosition>
	{
		public FiscalInventPositionMap()
		{
			Table("fiscal_invent_positions");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			References(x => x.OrderItem)
				.Column("order_item_id");

			Map(x => x.Name)
				.Column("name");

			Map(x => x.Quantity)
				.Column("quantity");

			Map(x => x.Price)
				.Column("price");

			Map(x => x.DiscountSum)
				.Column("discount_sum");

			Map(x => x.ProductMark)
				.Column("product_mark");

			Map(x => x.Vat)
				.Column("vat");

			References(x => x.RegulatoryDocument)
				.Column("regulatory_document_id");

			Map(x => x.IndustryRequisiteData)
				.Column("industry_requisite_data");
		}
	}
}
