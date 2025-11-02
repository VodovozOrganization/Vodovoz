using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OnlineOrderCancellationReasonMap : ClassMap<OnlineOrderCancellationReason>
	{
		public OnlineOrderCancellationReasonMap()
		{
			Table("online_order_cancellation_reasons");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
		}
	}
}
