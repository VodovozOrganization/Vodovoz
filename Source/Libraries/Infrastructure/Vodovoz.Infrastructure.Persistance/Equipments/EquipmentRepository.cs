using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.BasicHandbooks;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Equipments;

namespace Vodovoz.Infrastructure.Persistance.Equipments
{
	internal sealed class EquipmentRepository : IEquipmentRepository
	{
		#region С серийными номерами

		public QueryOver<Equipment> GetEquipmentWithKindsQuery(List<EquipmentKind> kinds)
		{
			Nomenclature nomenclatureAlias = null;
			var query = QueryOver.Of<Equipment>()
				.JoinAlias(e => e.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Kind.IsIn(kinds));

			return query;
		}

		public Equipment GetEquipmentForSaleByNomenclature(IUnitOfWork uow, Nomenclature nomenclature)
		{
			return AvailableEquipmentQuery().GetExecutableQueryOver(uow.Session)
				.Where(eq => eq.Nomenclature.Id == nomenclature.Id)
				.Where(eq => !eq.OnDuty)
				.Take(1)
				.List()
				.First();
		}

		public IList<Equipment> GetEquipmentForSaleByNomenclature(IUnitOfWork uow, Nomenclature nomenclature, int count = 0, int[] exceptIDs = null)
		{
			if(exceptIDs == null)
			{
				exceptIDs = new int[0];
			}

			return count > 0 ? AvailableEquipmentQuery().GetExecutableQueryOver(uow.Session)
				.Where(eq => eq.Nomenclature.Id == nomenclature.Id)
				.Where(eq => !eq.OnDuty)
				.Where(eq => !eq.Id.IsIn(exceptIDs))
				.Take(count)
				.List()
					: new List<Equipment>();
		}

		public Equipment GetAvailableEquipmentForRent(IUnitOfWork uow, EquipmentKind kind, int[] excludeEquipments)
		{
			Nomenclature nomenclatureAlias = null;

			//Ищем сначала дежурные
			var proposedList = AvailableOnDutyEquipmentQuery().GetExecutableQueryOver(uow.Session)
				.JoinAlias(e => e.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Kind == kind)
				.List();

			var result = FirstNotExcludedEquipment(proposedList, excludeEquipments);

			if(result != null)
			{
				return result;
			}

			//Выбираем сначала приоритетные модели
			proposedList = AvailableEquipmentQuery().GetExecutableQueryOver(uow.Session)
				.JoinAlias(e => e.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Kind == kind)
				.Where(() => nomenclatureAlias.RentPriority == true)
				.List();

			result = FirstNotExcludedEquipment(proposedList, excludeEquipments);

			if(result != null)
			{
				return result;
			}

			//Выбираем любой куллер
			proposedList = AvailableEquipmentQuery().GetExecutableQueryOver(uow.Session)
				.JoinAlias(e => e.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Kind == kind)
				.List();

			result = FirstNotExcludedEquipment(proposedList, excludeEquipments);

			return result;
		}

		private Equipment FirstNotExcludedEquipment(IList<Equipment> equipmentList, int[] excludeList)
		{
			return equipmentList.FirstOrDefault(e => !excludeList.Contains(e.Id));
		}

		public QueryOver<Equipment> AvailableOnDutyEquipmentQuery()
		{
			return AvailableEquipmentQuery().Where(equipment => equipment.OnDuty);
		}

		public QueryOver<Equipment, Equipment> AvailableEquipmentQuery()
		{
			Domain.Orders.Order orderAlias = null;
			Equipment equipmentAlias = null;
			WarehouseBulkGoodsAccountingOperation operationAddAlias = null;
			OrderEquipment orderEquipmentAlias = null;

			var equipmentInStockCriterion = Subqueries.IsNotNull(QueryOver.Of(() => operationAddAlias)
				.OrderBy(() => operationAddAlias.OperationTime).Desc
				.Where(() => equipmentAlias.Nomenclature.Id == operationAddAlias.Nomenclature.Id)
				.Select(op => op.Warehouse)
				.Take(1).DetachedCriteria);

			var subqueryAllReservedEquipment = QueryOver.Of(() => orderAlias)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Accepted
				   || orderAlias.OrderStatus == OrderStatus.InTravelList
				   || orderAlias.OrderStatus == OrderStatus.OnLoading) // чтобы не добавить в доп соглашение оборудование добавленное в уже созданный заказ.
				.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.Where(() => orderEquipmentAlias.Direction == Direction.Deliver)
				.Select(Projections.Property(() => orderEquipmentAlias.Equipment.Id));

			return QueryOver.Of(() => equipmentAlias)
				.Where(equipmentInStockCriterion)
				.Where(e => e.AssignedToClient == null)
				.WithSubquery.WhereProperty(() => equipmentAlias.Id).NotIn(subqueryAllReservedEquipment);
		}



		public QueryOver<Equipment> GetEquipmentByNomenclature(Nomenclature nomenclature)
		{
			Nomenclature nomenclatureAlias = null;

			return QueryOver.Of<Equipment>()
				.JoinAlias(e => e.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Id == nomenclature.Id);
		}

		public QueryOver<Equipment> GetEquipmentAtDeliveryPointQuery(Counterparty client, DeliveryPoint deliveryPoint)
		{
			Equipment equipmentAlias = null;
			CounterpartyMovementOperation operationAlias = null;
			CounterpartyMovementOperation subsequentOperationAlias = null;

			var subsequentOperationsSubquery = QueryOver.Of(() => subsequentOperationAlias)
				.Where(() => operationAlias.Id < subsequentOperationAlias.Id && operationAlias.Equipment == subsequentOperationAlias.Equipment)
				.Select(op => op.Id);

			var availableEquipmentIDsSubquery = QueryOver.Of(() => operationAlias)
				.WithSubquery.WhereNotExists(subsequentOperationsSubquery)
				.Where(() => operationAlias.IncomingCounterparty.Id == client.Id);

			if(deliveryPoint != null)
			{
				availableEquipmentIDsSubquery.Where(() => operationAlias.IncomingDeliveryPoint.Id == deliveryPoint.Id);
			}

			availableEquipmentIDsSubquery.Select(op => op.Equipment.Id);

			return QueryOver.Of(() => equipmentAlias).WithSubquery.WhereProperty(() => equipmentAlias.Id).In(availableEquipmentIDsSubquery);
		}

		public IList<Equipment> GetEquipmentAtDeliveryPoint(IUnitOfWork uow, DeliveryPoint deliveryPoint)
		{
			return GetEquipmentAtDeliveryPointQuery(deliveryPoint?.Counterparty, deliveryPoint)
					.GetExecutableQueryOver(uow.Session)
					.List();
		}

		public IList<Equipment> GetEquipmentForClient(IUnitOfWork uow, Counterparty counterparty)
		{
			return GetEquipmentAtDeliveryPointQuery(counterparty, null)
				.GetExecutableQueryOver(uow.Session)
				.List();
		}

		public EquipmentLocation GetLocation(IUnitOfWork uow, int equipmentId)
		{
			var result = new EquipmentLocation();

			var lastCouterpartyOp = uow.Session.QueryOver<CounterpartyMovementOperation>()
				.Where(o => o.Equipment.Id == equipmentId)
				.OrderBy(o => o.OperationTime).Desc
				.Take(1)
				.SingleOrDefault();

			if(lastCouterpartyOp == null)
			{
				result.Type = LocationType.NoMovements;
			}
			else if(lastCouterpartyOp.IncomingCounterparty != null)
			{
				result.Type = LocationType.Couterparty;
				result.Operation = lastCouterpartyOp;
				result.Counterparty = lastCouterpartyOp.IncomingCounterparty;
				result.DeliveryPoint = lastCouterpartyOp.IncomingDeliveryPoint;
			}
			else
			{
				result.Type = LocationType.Superposition;
			}

			return result;
		}

		#endregion
	}
}
