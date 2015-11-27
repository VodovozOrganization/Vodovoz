using System;
using Vodovoz.Domain;
using NHibernate.Criterion;

namespace Vodovoz.Repository
{
	public static class DeliveryScheduleRepository
	{
		public static QueryOver<DeliverySchedule> AllQuery ()
		{
			return QueryOver.Of<DeliverySchedule> ();
		}

	}
}

