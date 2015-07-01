using NHibernate.Criterion;
using Vodovoz.Domain;
using System.Collections.Generic;

namespace Vodovoz.Repository
{
	public static class DeliveryPointRepository
	{
		public static QueryOver<DeliveryPoint> DeliveryPointsForCounterpartyQuery (Counterparty counterparty)
		{
			var identifiers = new List<object> ();
			foreach (DeliveryPoint d in counterparty.DeliveryPoints)
				identifiers.Add (d.Id);
			return QueryOver.Of<DeliveryPoint> ()
				.Where (dp => dp.Id.IsIn (identifiers));
		}
	}
}

