using NHibernate.Criterion;
using Vodovoz.Domain;
using System.Collections.Generic;

namespace Vodovoz.Repository
{
	public static class DeliveryPointRepository
	{
		public static QueryOver<DeliveryPoint> DeliveryPointsForCounterpartyQuery (Counterparty counterparty)
		{
			return QueryOver.Of<DeliveryPoint> ()
				.Where (dp => dp.Counterparty.Id == counterparty.Id);
		}
	}
}

