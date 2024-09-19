using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class ReceiptEdoTaskMap : SubclassMap<ReceiptEdoTask>
	{
		public ReceiptEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.Receipt));

			Map(x => x.OrderId)
				.Column("order_id");
		}
	}
}
