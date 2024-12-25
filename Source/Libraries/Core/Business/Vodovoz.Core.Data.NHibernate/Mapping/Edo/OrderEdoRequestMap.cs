using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class OrderEdoRequestMap : SubclassMap<OrderEdoRequest>
	{
		public OrderEdoRequestMap()
		{
			DiscriminatorValue(nameof(CustomerEdoRequestType.Order));

			References(x => x.Order)
				.Column("order_id");
		}
	}
}
