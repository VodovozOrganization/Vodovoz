using System;
using System.Collections.Generic;
using System.Text;
using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Refunds;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Refund
{
	public class RefundMap : ClassMap<RefundEntity>
	{
		public RefundMap()
		{
			Table("refunds");
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Date).Column("date");

			References(x => x.Order).Column("order_id");
			References(x => x.OrderOnline).Column("online_order_id");
		}
		
	}
}
