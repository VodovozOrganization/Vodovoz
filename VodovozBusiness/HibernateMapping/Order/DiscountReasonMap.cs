using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.HibernateMapping.Order
{
	public class DiscountReasonMap : ClassMap<DiscountReason>
	{
		public DiscountReasonMap()
		{
			Table("discount_reasons");

			Id (x => x.Id)	.Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}
}
