using System;
using NHibernate.Criterion;
using Vodovoz.Domain.Logistic;

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

