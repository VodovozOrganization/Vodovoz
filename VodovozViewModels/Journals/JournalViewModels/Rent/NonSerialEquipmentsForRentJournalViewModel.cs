using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Journal.DataLoader;
using Vodovoz.Domain;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Nodes;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Rent
{
    public class NonSerialEquipmentsForRentJournalViewModel : JournalViewModelBase
    {
	    private readonly EquipmentKind _equipmentKind;
	    
        public NonSerialEquipmentsForRentJournalViewModel(
	        EquipmentKind equipmentKind,
            IUnitOfWorkFactory unitOfWorkFactory, 
            IInteractiveService interactiveService, 
            INavigationManager navigation) : base(unitOfWorkFactory, interactiveService, navigation)
        {
	        _equipmentKind = equipmentKind;
	        TabName = "Оборудование для аренды";
            
            var dataLoader = new ThreadDataLoader<NomenclatureForRentNode>(unitOfWorkFactory);
            dataLoader.AddQuery(ItemsQuery);
            DataLoader = dataLoader;

            SelectionMode = JournalSelectionMode.Single;
            CreateNodeActions();
        }

        private IQueryOver<Nomenclature> ItemsQuery(IUnitOfWork uow)
        {
            Nomenclature nomenclatureAlias = null;
			WarehouseMovementOperation operationAddAlias = null;

			//Подзапрос выбирающий по номенклатуре количество добавленное на склад
			var subqueryAdded = QueryOver.Of(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseMovementOperation>(o => o.IncomingWarehouse)))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));
			
			//Подзапрос выбирающий по номенклатуре количество отгруженное со склада
			var subqueryRemoved = QueryOver.Of(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseMovementOperation>(o => o.WriteoffWarehouse)))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			//Подзапрос выбирающий по номенклатуре количество зарезервированное в заказах до отгрузки со склада
			Order localOrderAlias = null;
			OrderEquipment localOrderEquipmentAlias = null;
			Equipment localEquipmentAlias = null;
			var subqueryReserved = QueryOver.Of(() => localOrderAlias)
				.JoinAlias(() => localOrderAlias.OrderEquipments, () => localOrderEquipmentAlias)
				.JoinAlias(() => localOrderEquipmentAlias.Equipment, () => localEquipmentAlias)
				.Where(() => localEquipmentAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(() => localOrderEquipmentAlias.Direction == Direction.Deliver)
											.Where(() => localOrderAlias.OrderStatus == OrderStatus.Accepted
												   || localOrderAlias.OrderStatus == OrderStatus.InTravelList
												   || localOrderAlias.OrderStatus == OrderStatus.OnLoading)
				.Select(Projections.Sum(() => localOrderEquipmentAlias.Count));

			NomenclatureForRentNode resultAlias = null;
			MeasurementUnits unitAlias = null;
			EquipmentKind equipmentKindAlias = null;

			//Запрос выбирающий количество добавленное на склад, отгруженное со склада 
			//и зарезервированное в заказах каждой номенклатуры по выбранному типу оборудования
			var query = uow.Session.QueryOver(() => nomenclatureAlias)
				.JoinAlias(() => nomenclatureAlias.Unit, () => unitAlias)
				.JoinAlias(() => nomenclatureAlias.Kind, () => equipmentKindAlias);
			
			if(_equipmentKind != null)
			{
				query = query.Where(() => nomenclatureAlias.Kind.Id == _equipmentKind.Id);
			}

			query.Where(GetSearchCriterion(() => nomenclatureAlias.Name));
			
			query = query.SelectList(list => list
							.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
							.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.NomenclatureName)
			                .Select(() => equipmentKindAlias.Id).WithAlias(() => resultAlias.TypeId)
							.Select(() => equipmentKindAlias).WithAlias(() => resultAlias.Kind)
							.Select(() => unitAlias.Name).WithAlias(() => resultAlias.UnitName)
							.Select(() => unitAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
							.SelectSubQuery(subqueryAdded).WithAlias(() => resultAlias.Added)
							.SelectSubQuery(subqueryRemoved).WithAlias(() => resultAlias.Removed)
							.SelectSubQuery(subqueryReserved).WithAlias(() => resultAlias.Reserved)
						   )
				.OrderBy(x => x.Name).Asc
				.TransformUsing(Transformers.AliasToBean<NomenclatureForRentNode>());
			
			return query;
        }
    }
}