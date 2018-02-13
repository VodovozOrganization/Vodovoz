﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using QSOrmProject;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Goods;
using NHibernate.Transform;
using QSOrmProject.RepresentationModel;
using QSBusinessCommon.Domain;

namespace Vodovoz.Repository
{
	public static class EquipmentRepository
	{

		#region Без серийных номеров

		/// <summary>
		/// Запрос выбирающий количество добавленное на склад, отгруженное со склада 
		/// и зарезервированное в заказах каждой номенклатуры по выбранному типу оборудования
		/// </summary>
		public static QueryOver<Nomenclature, Nomenclature> QueryAvailableNonSerialEquipmentForRent(EquipmentType type)
		{
			Nomenclature nomenclatureAlias = null;
			WarehouseMovementOperation operationAddAlias = null;

			//Подзапрос выбирающий по номенклатуре количество добавленное на склад
			var subqueryAdded = QueryOver.Of<WarehouseMovementOperation>(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseMovementOperation>(o => o.IncomingWarehouse)))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));
			
			//Подзапрос выбирающий по номенклатуре количество отгруженное со склада
			var subqueryRemoved = QueryOver.Of<WarehouseMovementOperation>(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseMovementOperation>(o => o.WriteoffWarehouse)))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			//Подзапрос выбирающий по номенклатуре количество зарезервированное в заказах до отгрузки со склада
			Vodovoz.Domain.Orders.Order localOrderAlias = null;
			OrderEquipment localOrderEquipmentAlias = null;
			Equipment localEquipmentAlias = null;
			var subqueryReserved = QueryOver.Of<Vodovoz.Domain.Orders.Order>(() => localOrderAlias)
				.JoinAlias(() => localOrderAlias.OrderEquipments, () => localOrderEquipmentAlias)
				.JoinAlias(() => localOrderEquipmentAlias.Equipment, () => localEquipmentAlias)
				.Where(() => localEquipmentAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(() => localOrderEquipmentAlias.Direction == Direction.Deliver)
											.Where(() => localOrderAlias.OrderStatus == OrderStatus.Accepted
												   || localOrderAlias.OrderStatus == OrderStatus.InTravelList
												   || localOrderAlias.OrderStatus == OrderStatus.OnLoading)
				.Select(Projections.Sum(() => localOrderEquipmentAlias.Count));

			NomenclatureForRentVMNode resultAlias = null;
			MeasurementUnits unitAlias = null;
			EquipmentType equipmentType = null;

			//Запрос выбирающий количество добавленное на склад, отгруженное со склада 
			//и зарезервированное в заказах каждой номенклатуры по выбранному типу оборудования
			var query = QueryOver.Of<Nomenclature>(() => nomenclatureAlias)
							 .JoinAlias(() => nomenclatureAlias.Unit, () => unitAlias)
							 .JoinAlias(() => nomenclatureAlias.Type, () => equipmentType);
			
			if(type != null){
				query = query.Where(() => nomenclatureAlias.Type.Id == type.Id);
			}

			query = query.SelectList(list => list
							.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
			                .Select(() => equipmentType.Id).WithAlias(() => resultAlias.TypeId)
							.Select(() => unitAlias.Name).WithAlias(() => resultAlias.UnitName)
							.Select(() => unitAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
							.SelectSubQuery(subqueryAdded).WithAlias(() => resultAlias.Added)
							.SelectSubQuery(subqueryRemoved).WithAlias(() => resultAlias.Removed)
							.SelectSubQuery(subqueryReserved).WithAlias(() => resultAlias.Reserved)
						   )
				.TransformUsing(Transformers.AliasToBean<NomenclatureForRentVMNode>());
			return query;
		}

		/// <summary>
		/// Возвращает доступное оборудование указанного типа для аренды
		/// </summary>
		public static Nomenclature GetAvailableNonSerialEquipmentForRent(IUnitOfWork uow, EquipmentType type, int[] excludeNomenclatures)
		{
			Nomenclature nomenclatureAlias = null;

			var nomenclatureList = GetAllNonSerialEquipmentForRent(uow, type);
			
			//Выбираются только доступные на складе и еще не выбранные в диалоге
			var availableNomenclature = nomenclatureList.Where(x => x.Available > 0)
			                                            .Where(x => !excludeNomenclatures.Contains(x.Id));

			//Если есть дежурное оборудование выбираем сначала его
			var duty = uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
							  .Where(() => nomenclatureAlias.IsDuty)
							  .WhereRestrictionOn(() => nomenclatureAlias.Id)
							  .IsIn(availableNomenclature.Select(x => x.Id).ToArray()).List();
			if(duty.Count() > 0) {
				return duty.First();
			}

			//Иначе если есть приоритетное оборудование выбираем его
			var priority = uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
							  .Where(() => nomenclatureAlias.RentPriority)
							  .WhereRestrictionOn(() => nomenclatureAlias.Id)
							  .IsIn(availableNomenclature.Select(x => x.Id).ToArray()).List();
			if(priority.Count() > 0) {
				return priority.First();
			}

			//Выбираем любое доступное оборудование
			var any = uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
							  .WhereRestrictionOn(() => nomenclatureAlias.Id)
							  .IsIn(availableNomenclature.Select(x => x.Id).ToArray()).List();
			return any.FirstOrDefault();
		}

		/// <summary>
		/// Возвращает список всего оборудования определенного типа для аренды
		/// </summary>
		public static IList<NomenclatureForRentVMNode> GetAllNonSerialEquipmentForRent(IUnitOfWork uow, EquipmentType type)
		{
			var result = QueryAvailableNonSerialEquipmentForRent(type)
				.GetExecutableQueryOver(uow.Session)
				.List<NomenclatureForRentVMNode>();
			return FillNodeObjects(uow, result);
		}

		/// <summary>
		/// Возвращает список всего оборудования для аренды
		/// </summary>
		public static IList<NomenclatureForRentVMNode> GetAllNonSerialEquipmentForRent(IUnitOfWork uow)
		{
			var result = QueryAvailableNonSerialEquipmentForRent(null)
				.GetExecutableQueryOver(uow.Session)
				.List<NomenclatureForRentVMNode>();
			return FillNodeObjects(uow, result);
		}

		/// <summary>
		/// Заполняет номенклатуру и тип реальными объектами по id
		/// </summary>
		private static IList<NomenclatureForRentVMNode> FillNodeObjects(IUnitOfWork uow, IList<NomenclatureForRentVMNode> nodes)
		{
			var nomList = uow.Session.QueryOver<Nomenclature>()
							 .WhereRestrictionOn(x => x.Id)
							 .IsIn(nodes.Select(x => x.Id).ToArray())
							 .List().ToDictionary(x => x.Id);
			var typeList = uow.Session.QueryOver<EquipmentType>()
							 .WhereRestrictionOn(x => x.Id)
							 .IsIn(nodes.Select(x => x.TypeId).ToArray())
							 .List().ToDictionary(x => x.Id);

			foreach(var node in nodes) {
				node.Nomenclature = nomList[node.Id];
				node.Type = typeList[node.TypeId];
			}
			return nodes;
		}

		/// <summary>
		/// Выбирает первое имееющееся в справочнике оборудование по выбранному типу
		/// </summary>
		public static Nomenclature GetFirstAnyNomenclatureForRent(IUnitOfWork uow, EquipmentType type)
		{
			Nomenclature nomenclatureAlias = null;
			return uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
			   .Where(() => nomenclatureAlias.Type == type)
			   .List().FirstOrDefault();
		}

		#endregion


		#region С серийными номерами

		public static QueryOver<Equipment> GetEquipmentWithTypesQuery(List<EquipmentType> types)
		{
			Nomenclature nomenclatureAlias = null;
			var Query = QueryOver.Of<Equipment>()
				.JoinAlias(e => e.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Type.IsIn(types));
			return Query;
		}

		public static Equipment GetEquipmentForSaleByNomenclature(IUnitOfWork uow, Nomenclature nomenclature)
		{
			return AvailableEquipmentQuery().GetExecutableQueryOver(uow.Session)
				.Where(eq => eq.Nomenclature.Id == nomenclature.Id)
				.Where(eq => !eq.OnDuty)
				.Take(1)
				.List().First();
		}

		public static IList<Equipment> GetEquipmentForSaleByNomenclature(IUnitOfWork uow, Nomenclature nomenclature, int count = 0, int[] exceptIDs = null)
		{
			if(exceptIDs == null) exceptIDs = new int[0];
			return (count > 0) ? AvailableEquipmentQuery().GetExecutableQueryOver(uow.Session)
				.Where(eq => eq.Nomenclature.Id == nomenclature.Id)
				.Where(eq => !eq.OnDuty)
				.Where(eq => !eq.Id.IsIn(exceptIDs))
				.Take(count)
				.List()
					: new List<Equipment>();
		}

		public static Equipment GetAvailableEquipmentForRent(IUnitOfWork uow, EquipmentType type, int[] excludeEquipments)
		{
			Nomenclature nomenclatureAlias = null;

			//Ищем сначала дежурные
			var proposedList = AvailableOnDutyEquipmentQuery().GetExecutableQueryOver(uow.Session)
				.JoinAlias(e => e.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Type == type)
				.List();

			var result = FirstNotExcludedEquipment(proposedList, excludeEquipments);

			if(result != null)
				return result;

			//Выбираем сначала приоритетные модели
			proposedList = AvailableEquipmentQuery().GetExecutableQueryOver(uow.Session)
				.JoinAlias(e => e.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Type == type)
				.Where(() => nomenclatureAlias.RentPriority == true)
				.List();

			result = FirstNotExcludedEquipment(proposedList, excludeEquipments);

			if(result != null)
				return result;

			//Выбираем любой куллер
			proposedList = AvailableEquipmentQuery().GetExecutableQueryOver(uow.Session)
				.JoinAlias(e => e.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Type == type)
				.List();

			result = FirstNotExcludedEquipment(proposedList, excludeEquipments);

			return result;
		}

		private static Equipment FirstNotExcludedEquipment(IList<Equipment> equipmentList, int[] excludeList)
		{
			return equipmentList.FirstOrDefault(e => !excludeList.Contains(e.Id));
		}

		public static QueryOver<Equipment> AvailableOnDutyEquipmentQuery()
		{
			return AvailableEquipmentQuery().Where(equipment => equipment.OnDuty);
		}

		public static QueryOver<Equipment, Equipment> AvailableEquipmentQuery()
		{
			Vodovoz.Domain.Orders.Order orderAlias = null;
			Equipment equipmentAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			OrderEquipment orderEquipmentAlias = null;

			var equipmentInStockCriterion = Subqueries.IsNotNull(
												QueryOver.Of<WarehouseMovementOperation>(() => operationAddAlias)
				.OrderBy(() => operationAddAlias.OperationTime).Desc
				.Where(() => equipmentAlias.Id == operationAddAlias.Equipment.Id)
				.Select(op => op.IncomingWarehouse)
				.Take(1).DetachedCriteria
											);

			var subqueryAllReservedEquipment = QueryOver.Of<Vodovoz.Domain.Orders.Order>(() => orderAlias)
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



		public static QueryOver<Equipment> GetEquipmentByNomenclature(Nomenclature nomenclature)
		{
			Nomenclature nomenclatureAlias = null;

			return QueryOver.Of<Equipment>()
				.JoinAlias(e => e.Nomenclature, () => nomenclatureAlias)
				.Where(() => nomenclatureAlias.Id == nomenclature.Id);
		}

		public static QueryOver<Equipment> GetEquipmentAtDeliveryPointQuery(Counterparty client, DeliveryPoint deliveryPoint)
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
				availableEquipmentIDsSubquery
					.Where(() => operationAlias.IncomingDeliveryPoint.Id == deliveryPoint.Id);
			availableEquipmentIDsSubquery
				.Select(op => op.Equipment.Id);
			return QueryOver.Of<Equipment>(() => equipmentAlias).WithSubquery.WhereProperty(() => equipmentAlias.Id).In(availableEquipmentIDsSubquery);
		}

		public static IList<Equipment> GetEquipmentAtDeliveryPoint(IUnitOfWork uow, DeliveryPoint deliveryPoint)
		{
			return GetEquipmentAtDeliveryPointQuery(deliveryPoint.Counterparty, deliveryPoint)
				.GetExecutableQueryOver(uow.Session)
				.List();
		}

		public static IList<Equipment> GetEquipmentForClient(IUnitOfWork uow, Counterparty counterparty)
		{
			return GetEquipmentAtDeliveryPointQuery(counterparty, null)
				.GetExecutableQueryOver(uow.Session)
				.List();
		}

		public static QueryOver<Equipment> GetUnusedEquipment(Nomenclature nomenclature)
		{
			Equipment equipmantAlias = null;

			var counterpartyOperationsSubquery = QueryOver.Of<CounterpartyMovementOperation>()
				.Where(op => op.Equipment.Id == equipmantAlias.Id)
				.Select(op => op.Id);

			var warehouseOperationsSubquery = QueryOver.Of<WarehouseMovementOperation>()
				.Where(op => op.Equipment.Id == equipmantAlias.Id)
				.Select(op => op.Id);

			return QueryOver.Of<Equipment>(() => equipmantAlias)
				.Where(() => equipmantAlias.Nomenclature.Id == nomenclature.Id)
				.WithSubquery.WhereNotExists(counterpartyOperationsSubquery)
				.WithSubquery.WhereNotExists(warehouseOperationsSubquery);
		}

		public static IList<Equipment> GetEquipmentUnloadedTo(IUnitOfWork uow, RouteList routeList)
		{
			CarUnloadDocumentItem unloadItemAlias = null;
			WarehouseMovementOperation operationAlias = null;
			Equipment equipmentAlias = null;
			var unloadedEquipmentIdsQuery = QueryOver.Of<CarUnloadDocument>().Where(doc => doc.RouteList.Id == routeList.Id)
				.JoinAlias(doc => doc.Items, () => unloadItemAlias)
				.JoinAlias(() => unloadItemAlias.MovementOperation, () => operationAlias)
				.JoinAlias(() => operationAlias.Equipment, () => equipmentAlias)
				.Select(op => equipmentAlias.Id);
			return uow.Session.QueryOver<Equipment>(() => equipmentAlias).WithSubquery.WhereProperty(() => equipmentAlias.Id).In(unloadedEquipmentIdsQuery).List();
		}

		public static EquipmentLocation GetLocation(IUnitOfWork uow, Equipment equ)
		{
			var result = new EquipmentLocation();
			var lastWarehouseOp = uow.Session.QueryOver<WarehouseMovementOperation>()
				.Where(o => o.Equipment == equ)
				.OrderBy(o => o.OperationTime).Desc
				.Take(1)
				.SingleOrDefault();
			var lastCouterpartyOp = uow.Session.QueryOver<CounterpartyMovementOperation>()
				.Where(o => o.Equipment == equ)
				.OrderBy(o => o.OperationTime).Desc
				.Take(1)
				.SingleOrDefault();

			if(lastWarehouseOp == null && lastCouterpartyOp == null)
				result.Type = EquipmentLocation.LocationType.NoMovements;
			else if(lastWarehouseOp != null && lastWarehouseOp.IncomingWarehouse != null
				&& (lastCouterpartyOp == null || lastCouterpartyOp.OperationTime < lastWarehouseOp.OperationTime)) {
				result.Type = EquipmentLocation.LocationType.Warehouse;
				result.Warehouse = lastWarehouseOp.IncomingWarehouse;
				result.Operation = lastWarehouseOp;
			} else if(lastCouterpartyOp != null && lastCouterpartyOp.IncomingCounterparty != null
				  && (lastWarehouseOp == null || lastWarehouseOp.OperationTime < lastCouterpartyOp.OperationTime)) {
				result.Type = EquipmentLocation.LocationType.Couterparty;
				result.Operation = lastCouterpartyOp;
				result.Counterparty = lastCouterpartyOp.IncomingCounterparty;
				result.DeliveryPoint = lastCouterpartyOp.IncomingDeliveryPoint;
			} else {
				result.Type = EquipmentLocation.LocationType.Superposition;
			}

			return result;
		}

		public class EquipmentLocation
		{

			public LocationType Type { get; set; }

			public OperationBase Operation { get; set; }

			public Warehouse Warehouse { get; set; }

			public Counterparty Counterparty { get; set; }

			public DeliveryPoint DeliveryPoint { get; set; }

			public enum LocationType
			{
				NoMovements,
				Warehouse,
				Couterparty,
				Superposition //Like Quantum
			}

			public string Title {
				get {
					switch(Type) {
						case LocationType.NoMovements:
							return "Нет движений в БД";
						case LocationType.Warehouse:
							return String.Format("На складе: {0}", Warehouse.Name);
						case LocationType.Couterparty:
							return String.Format("У {0}{1}", Counterparty.Name,
								DeliveryPoint != null ? " на адресе " + DeliveryPoint.Title : String.Empty);
						case LocationType.Superposition:
							return "В состоянии суперпозиции (как кот Шрёдингера)";
					}
					return null;
				}
			}
		}

		#endregion

	}

	public class NomenclatureForRentVMNode
	{
		public int Id { get; set; }
		public Nomenclature Nomenclature { get; set; }
		[UseForSearch]
		public int TypeId { get; set; }
		public EquipmentType Type { get; set; }
		public decimal InStock { get { return Added - Removed; } }
		public int? Reserved { get; set; }
		public decimal Available { get { return InStock - Reserved.GetValueOrDefault(); } }
		public decimal Added { get; set; }
		public decimal Removed { get; set; }
		public string UnitName { get; set; }
		public short UnitDigits { get; set; }
		public bool IsEquipmentWithSerial { get; set; }
		private string Format(decimal value)
		{
			return String.Format("{0:F" + UnitDigits + "} {1}", value, UnitName);
		}

		public string InStockText { get { return Format(InStock); } }
		public string ReservedText { get { return Format(Reserved.GetValueOrDefault()); } }
		public string AvailableText { get { return Format(Available); } }
	}
}

