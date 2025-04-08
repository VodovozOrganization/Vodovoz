using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Logistics;

namespace Vodovoz.Core.Data.NHibernate.Logistic
{
	public class DeliveryScheduleMap : ClassMap<DeliveryScheduleEntity>
	{
		public DeliveryScheduleMap()
		{
			Table("delivery_schedule");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
		}
	}
}
