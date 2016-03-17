using NHibernate.Criterion;
using Vodovoz.Domain;
using System.Collections.Generic;
using QSOrmProject;
using Vodovoz.Domain.Operations;
using NHibernate.Transform;
using System.Linq;

namespace Vodovoz.Repository
{
	public static class DeliveryPointRepository
	{
		public static QueryOver<DeliveryPoint> DeliveryPointsForCounterpartyQuery (Counterparty counterparty)
		{
			return QueryOver.Of<DeliveryPoint> ()
				.Where (dp => dp.Counterparty.Id == counterparty.Id);
		}

		public static int GetBottlesAtDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint)
		{
			BottlesMovementOperation operationAlias = null;
			BottlesAtDeliveryPointQueryResult result;
			var queryResult = UoW.Session.QueryOver<BottlesMovementOperation>(() => operationAlias)
				.Where(() => operationAlias.DeliveryPoint.Id == deliveryPoint.Id)
				.SelectList(list => list
					.SelectSum(()=>operationAlias.Delivered).WithAlias(()=>result.Delivered)
					.SelectSum(()=>operationAlias.Returned).WithAlias(()=>result.Returned)
				)
				.TransformUsing(Transformers.AliasToBean<BottlesAtDeliveryPointQueryResult>()).List<BottlesAtDeliveryPointQueryResult>();
			var bottleCount = queryResult.FirstOrDefault()?.Total ?? 0;
			return bottleCount;
		}

		class BottlesAtDeliveryPointQueryResult
		{
			public int Delivered{ get; set; }
			public int Returned{get;set;}
			public int Total
			{
				get{ return Delivered - Returned; }
			}
		}
	}
}

