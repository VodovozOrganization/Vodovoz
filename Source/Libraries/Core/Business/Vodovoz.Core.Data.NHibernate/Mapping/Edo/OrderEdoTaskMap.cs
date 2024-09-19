using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class OrderEdoTaskMap : SubclassMap<OrderEdoTask>
	{
		public OrderEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.Order));

			Map(x => x.OrderId)
				.Column("order_id");
		}
	}
}
