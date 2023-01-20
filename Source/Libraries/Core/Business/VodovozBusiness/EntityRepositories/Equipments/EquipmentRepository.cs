using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Equipments
{
	public class EquipmentRepository : IEquipmentRepository
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

			return (count > 0) ? AvailableEquipmentQuery().GetExecutableQueryOver(uow.Session)
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

		//TODO Проверить работу запроса
		public QueryOver<Equipment, Equipment> AvailableEquipmentQuery()
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Equipment equipmentAlias = null;
			WarehouseBulkGoodsAccountingOperation operationAddAlias = null;
			OrderEquipment orderEquipmentAlias = null;

			var equipmentInStockCriterion = Subqueries.IsNotNull(QueryOver.Of(() => operationAddAlias)
				.OrderBy(() => operationAddAlias.OperationTime).Desc
				.Where(() => equipmentAlias.Nomenclature.Id == operationAddAlias.Nomenclature.Id)
				.Select(op => op.Warehouse)
				.Take(1).DetachedCriteria
											);

			var subqueryAllReservedEquipment = QueryOver.Of(() => orderAlias)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Accepted
				   || orderAlias.OrderStatus == OrderStatus.InTravelList
				   || orderAlias.OrderStatus == OrderStatus.OnLoading) // чтобы не добавить в доп соглашение оборудование добавленное в уже созданный заказ.
				.JoinAlias(() => orderAlias.OrderEquipments, () => orderEquipmentAlias)
				.Where(() => orderEquipmentAlias.Direction == Direction.Deliver)
				.Select(Projections.Property(() => orderEquipmentAlias.Equipment.Id));

			return QueryOver.Of<Equipment>(() => equipmentAlias)
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

			var subsequentOperationsSubquery = QueryOver.Of<CounterpartyMovementOperation>(() => subsequentOperationAlias)
				.Where(() => operationAlias.Id < subsequentOperationAlias.Id && operationAlias.Equipment == subsequentOperationAlias.Equipment)
				.Select(op => op.Id);

			var availableEquipmentIDsSubquery = QueryOver.Of<CounterpartyMovementOperation>(() => operationAlias)
				.WithSubquery.WhereNotExists(subsequentOperationsSubquery)
				.Where(() => operationAlias.IncomingCounterparty.Id == client.Id);
			
			if(deliveryPoint != null)
			{
				availableEquipmentIDsSubquery.Where(() => operationAlias.IncomingDeliveryPoint.Id == deliveryPoint.Id);
			}

			availableEquipmentIDsSubquery.Select(op => op.Equipment.Id);
			
			return QueryOver.Of<Equipment>(() => equipmentAlias).WithSubquery.WhereProperty(() => equipmentAlias.Id).In(availableEquipmentIDsSubquery);
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

		//TODO удалить после теста
		public QueryOver<Equipment> GetUnusedEquipment(Nomenclature nomenclature)
		{
			return null;
			/*Equipment equipmantAlias = null;

			var counterpartyOperationsSubquery = QueryOver.Of<CounterpartyMovementOperation>()
				.Where(op => op.Equipment.Id == equipmantAlias.Id)
				.Select(op => op.Id);

			var warehouseOperationsSubquery = QueryOver.Of<GoodsAccountingOperation>()
				.Where(op => op.Equipment.Id == equipmantAlias.Id)
				.Select(op => op.Id);

			return QueryOver.Of<Equipment>(() => equipmantAlias)
				.Where(() => equipmantAlias.Nomenclature.Id == nomenclature.Id)
				.WithSubquery.WhereNotExists(counterpartyOperationsSubquery)
				.WithSubquery.WhereNotExists(warehouseOperationsSubquery);*/
		}

		//TODO проверить работу запроса
		public IList<Equipment> GetEquipmentUnloadedTo(IUnitOfWork uow, RouteList routeList)
		{
			CarUnloadDocumentItem unloadItemAlias = null;
			GoodsAccountingOperation operationAlias = null;
			Equipment equipmentAlias = null;
			
			var unloadedEquipmentIdsQuery = QueryOver.Of<CarUnloadDocument>().Where(doc => doc.RouteList.Id == routeList.Id)
				.JoinAlias(doc => doc.Items, () => unloadItemAlias)
				.JoinAlias(() => unloadItemAlias.GoodsAccountingOperation, () => operationAlias)
				//.JoinAlias(() => operationAlias.Equipment, () => equipmentAlias)
				.Select(op => equipmentAlias.Id);
			
			return uow.Session.QueryOver<Equipment>(() => equipmentAlias)
				.WithSubquery.WhereProperty(() => equipmentAlias.Id)
				.In(unloadedEquipmentIdsQuery)
				.List();
		}

		//TODO проверить работу запроса
		public EquipmentLocation GetLocation(IUnitOfWork uow, Equipment equ)
		{
			var result = new EquipmentLocation();
			
			var lastWarehouseOp = uow.Session.QueryOver<WarehouseBulkGoodsAccountingOperation>()
				//.Where(o => o.Equipment == equ)
				.OrderBy(o => o.OperationTime).Desc
				.Take(1)
				.SingleOrDefault();
			
			var lastCouterpartyOp = uow.Session.QueryOver<CounterpartyMovementOperation>()
				.Where(o => o.Equipment == equ)
				.OrderBy(o => o.OperationTime).Desc
				.Take(1)
				.SingleOrDefault();

			if(lastWarehouseOp == null && lastCouterpartyOp == null)
			{
				result.Type = LocationType.NoMovements;
			}
			else if(lastWarehouseOp?.Warehouse != null && lastWarehouseOp.Amount > 0
			        && (lastCouterpartyOp == null || lastCouterpartyOp.OperationTime < lastWarehouseOp.OperationTime))
			{
				result.Type = LocationType.Warehouse;
				result.Warehouse = lastWarehouseOp.Warehouse;
				result.Operation = lastWarehouseOp;
			}
			else if(lastCouterpartyOp?.IncomingCounterparty != null
			        && (lastWarehouseOp == null || lastWarehouseOp.OperationTime < lastCouterpartyOp.OperationTime))
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

