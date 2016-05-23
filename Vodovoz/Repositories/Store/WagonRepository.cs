using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Repository.Store
{
	public static class WagonRepository
	{
		public static IList<MovementWagon> UsedWagonsByPeriod(IUnitOfWork uow, DateTime start, DateTime end, Warehouse warehouse)
		{
			WarehouseMovementOperation shipOpAlias = null;
			WarehouseMovementOperation deliverOpAlias = null;
			MovementDocumentItem itemAlias = null;
			MovementDocument docAlias = null;
			MovementWagon wagonAlias = null;
			end = end.Date.AddDays(1);

			var docSubqery = QueryOver.Of<MovementDocument>(() => docAlias)
				.JoinAlias(d => d.Items, () => itemAlias)
				.JoinAlias(() => itemAlias.WarehouseMovementOperation, () => shipOpAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.JoinAlias(() => itemAlias.DeliveryMovementOperation, () => deliverOpAlias, NHibernate.SqlCommand.JoinType.LeftOuterJoin)
				.Where(d => d.MovementWagon.Id == wagonAlias.Id)
				.Where(() => ((shipOpAlias.IncomingWarehouse == warehouse || shipOpAlias.WriteoffWarehouse == warehouse)
				                 && shipOpAlias.OperationTime >= start && shipOpAlias.OperationTime < end)
				                 || ((deliverOpAlias.IncomingWarehouse == warehouse || deliverOpAlias.WriteoffWarehouse == warehouse)
				                 && deliverOpAlias.OperationTime >= start && deliverOpAlias.OperationTime < end))
				.Select(d => d.Id);

			return uow.Session.QueryOver<MovementWagon>(() => wagonAlias)
				.WithSubquery.WhereExists(docSubqery).List();
		}
	}
}

