using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class DeliveryShiftMap : ClassMap<DeliveryShift>
	{
		public DeliveryShiftMap ()
		{
			Table("delivery_shift");
			Not.LazyLoad ();

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Name).Column ("name");
		}
	}
}

