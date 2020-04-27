using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.RepresentationModel.GtkUI;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.TempAdapters
{
    public static class EquipmentRepositoryForViews
    {
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
			if(duty.Any()) {
				return duty.First();
			}

			//Иначе если есть приоритетное оборудование выбираем его
			var priority = uow.Session.QueryOver<Nomenclature>(() => nomenclatureAlias)
							  .Where(() => nomenclatureAlias.RentPriority)
							  .WhereRestrictionOn(() => nomenclatureAlias.Id)
							  .IsIn(availableNomenclature.Select(x => x.Id).ToArray()).List();
			if(priority.Any()) {
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
        
        public class NomenclatureForRentVMNode
        {
            public int Id { get; set; }
            public Nomenclature Nomenclature { get; set; }
            [UseForSearch]
            public int TypeId { get; set; }
            public EquipmentType Type { get; set; }
            public decimal InStock => Added - Removed;
            public int? Reserved { get; set; }
            public decimal Available => InStock - Reserved.GetValueOrDefault();
            public decimal Added { get; set; }
            public decimal Removed { get; set; }
            public string UnitName { get; set; }
            public short UnitDigits { get; set; }
            private string Format(decimal value)
            {
                return String.Format("{0:F" + UnitDigits + "} {1}", value, UnitName);
            }
            public string InStockText => Format(InStock);
            public string ReservedText => Format(Reserved.GetValueOrDefault());
            public string AvailableText => Format(Available);
        }
    }
}