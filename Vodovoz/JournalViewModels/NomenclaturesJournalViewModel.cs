using System;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.BusinessCommon.Domain;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Project.Journal.EntitySelector;
using QS.Services;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalNodes;
using Vodovoz.ViewModels.Goods;
using VodovozOrder = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.JournalViewModels
{
	public class NomenclaturesJournalViewModel : FilterableSingleEntityJournalViewModelBase<Nomenclature, NomenclatureViewModel, NomenclatureJournalNode, NomenclatureFilterViewModel>
	{
		private readonly int currentUserId;
		private readonly IEmployeeService employeeService;
		private readonly IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory;
		private readonly IEntityAutocompleteSelectorFactory counterpartySelectorFactory;
		private readonly INomenclatureRepository nomenclatureRepository;
		private readonly IUserRepository userRepository;

		public NomenclaturesJournalViewModel(
			NomenclatureFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeService employeeService,
			IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory,
			IEntityAutocompleteSelectorFactory counterpartySelectorFactory,
			INomenclatureRepository nomenclatureRepository,
			IUserRepository userRepository
		) : base(filterViewModel, unitOfWorkFactory, commonServices) 
		{
			this.employeeService = employeeService ?? throw new ArgumentNullException(nameof(employeeService));
			this.nomenclatureSelectorFactory = nomenclatureSelectorFactory ?? throw new ArgumentNullException(nameof(nomenclatureSelectorFactory));
			this.counterpartySelectorFactory = counterpartySelectorFactory ?? throw new ArgumentNullException(nameof(counterpartySelectorFactory));
			this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));
			this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

			TabName = "Журнал ТМЦ";
			this.currentUserId = commonServices.UserService.CurrentUserId;
			SetOrder(x => x.Name);
			UpdateOnChanges(
				typeof(Nomenclature),
				typeof(MeasurementUnits),
				typeof(WarehouseMovementOperation),
				typeof(VodovozOrder),
				typeof(OrderItem)
			);
		}

		[Obsolete("Лучше передавать через фильтр")]
		public int[] ExcludingNomenclatureIds { get; set; }

		public IAdditionalJournalRestriction<Nomenclature> AdditionalJournalRestriction { get; set; } = null;

		protected override Func<IUnitOfWork, IQueryOver<Nomenclature>> ItemsSourceQueryFunction => (uow) => {
			var canAddSpares = commonServices.PermissionService.ValidateUserPresetPermission("can_add_spares_to_order", currentUserId);
			var canAddBottles = commonServices.PermissionService.ValidateUserPresetPermission("can_add_bottles_to_order", currentUserId);
			var canAddMaterials = commonServices.PermissionService.ValidateUserPresetPermission("can_add_materials_to_order", currentUserId);
			var canAddEquipmentNotForSale = commonServices.PermissionService.ValidateUserPresetPermission("can_add_equipment_not_for_sale_to_order", currentUserId);

			Nomenclature nomenclatureAlias = null;
			MeasurementUnits unitAlias = null;
			NomenclatureJournalNode resultAlias = null;
			WarehouseMovementOperation operationAddAlias = null;
			WarehouseMovementOperation operationRemoveAlias = null;
			Nomenclature loadedWarehouseAlias = null;
			VodovozOrder orderAlias = null;
			OrderItem orderItemsAlias = null;

			var subqueryAdded = QueryOver.Of(() => operationAddAlias)
				.Where(() => operationAddAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseMovementOperation>(o => o.IncomingWarehouse)))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			var subqueryRemoved = QueryOver.Of(() => operationRemoveAlias)
				.Where(() => operationRemoveAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(Restrictions.IsNotNull(Projections.Property<WarehouseMovementOperation>(o => o.WriteoffWarehouse)))
				.Select(Projections.Sum<WarehouseMovementOperation>(o => o.Amount));

			var subqueryReserved = QueryOver.Of(() => orderAlias)
				.JoinAlias(() => orderAlias.OrderItems, () => orderItemsAlias)
				.Where(() => orderItemsAlias.Nomenclature.Id == nomenclatureAlias.Id)
				.Where(() => nomenclatureAlias.DoNotReserve == false)
				.Where(() => orderAlias.OrderStatus == OrderStatus.Accepted
					   || orderAlias.OrderStatus == OrderStatus.InTravelList
					   || orderAlias.OrderStatus == OrderStatus.OnLoading)
				.Select(Projections.Sum(() => orderItemsAlias.Count));

			var itemsQuery = uow.Session.QueryOver(() => nomenclatureAlias)
								.Where(() => !nomenclatureAlias.IsArchive)
								;

			if(!FilterViewModel.RestrictArchive)
				itemsQuery.Where(() => !nomenclatureAlias.IsArchive);

			if(FilterViewModel.RestrictedExcludedIds != null && FilterViewModel.RestrictedExcludedIds.Any()) {
				itemsQuery.WhereNot(() => nomenclatureAlias.Id.IsIn(FilterViewModel.RestrictedExcludedIds.ToArray()));
			}

			if(FilterViewModel.RestrictedLoadedWarehouse != null) {
				itemsQuery
				.Left.JoinAlias(() => nomenclatureAlias.Warehouses, () => loadedWarehouseAlias)
				.Where(() => loadedWarehouseAlias.Id == FilterViewModel.RestrictedLoadedWarehouse.Id);
			}

			if(ExcludingNomenclatureIds != null && ExcludingNomenclatureIds.Any())
				itemsQuery.WhereNot(() => nomenclatureAlias.Id.IsIn(ExcludingNomenclatureIds));

			itemsQuery.Where(
				GetSearchCriterion(
					() => nomenclatureAlias.Name,
					() => nomenclatureAlias.Id,
					() => nomenclatureAlias.OnlineStoreExternalId
				)
			);

			if(!FilterViewModel.RestrictDilers)
				itemsQuery.Where(() => !nomenclatureAlias.IsDiler);
			if(FilterViewModel.RestrictCategory == NomenclatureCategory.water)
				itemsQuery.Where(() => nomenclatureAlias.IsDisposableTare == FilterViewModel.RestrictDisposbleTare);

			if(FilterViewModel.RestrictCategory.HasValue)
				itemsQuery.Where(n => n.Category == FilterViewModel.RestrictCategory.Value);

			if(FilterViewModel.SelectCategory.HasValue && FilterViewModel.SelectSaleCategory.HasValue && Nomenclature.GetCategoriesWithSaleCategory().Contains(FilterViewModel.SelectCategory.Value))
				itemsQuery.Where(n => n.SaleCategory == FilterViewModel.SelectSaleCategory);

			if(AdditionalJournalRestriction != null)
				foreach(var expr in AdditionalJournalRestriction.ExternalRestrictions)
					itemsQuery.Where(expr);

			if(AdditionalJournalRestriction is NomenclaturesForOrderJournalRestriction restr && restr.CalculateQtyOnStock) {
				itemsQuery.Left.JoinAlias(() => nomenclatureAlias.Unit, () => unitAlias)
					.Where(() => !nomenclatureAlias.IsSerial)
					.SelectList(list => list
						.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.Category)
						.Select(() => unitAlias.Name).WithAlias(() => resultAlias.UnitName)
						.Select(() => unitAlias.Digits).WithAlias(() => resultAlias.UnitDigits)
						.Select(() => nomenclatureAlias.OnlineStoreExternalId).WithAlias(() => resultAlias.OnlineStoreExternalId)
						.SelectSubQuery(subqueryAdded).WithAlias(() => resultAlias.Added)
						.SelectSubQuery(subqueryRemoved).WithAlias(() => resultAlias.Removed)
						.SelectSubQuery(subqueryReserved).WithAlias(() => resultAlias.Reserved)
					)
					.OrderBy(x => x.Name).Asc
					.TransformUsing(Transformers.AliasToBean<NomenclatureJournalNode>())
					;
			} else {
				itemsQuery.Where(() => !nomenclatureAlias.IsSerial)
					.SelectList(list => list
						.SelectGroup(() => nomenclatureAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => nomenclatureAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => nomenclatureAlias.Category).WithAlias(() => resultAlias.Category)
						.Select(() => nomenclatureAlias.OnlineStoreExternalId).WithAlias(() => resultAlias.OnlineStoreExternalId)
						.Select(() => false).WithAlias(() => resultAlias.CalculateQtyOnStock)
					)
					.OrderBy(x => x.Name).Asc
					.TransformUsing(Transformers.AliasToBean<NomenclatureJournalNode>())
					;
			}

			return itemsQuery;
		};

		protected override Func<NomenclatureViewModel> CreateDialogFunction =>
			() => new NomenclatureViewModel(EntityUoWBuilder.ForCreate(), UnitOfWorkFactory, commonServices,
				employeeService, nomenclatureSelectorFactory, counterpartySelectorFactory, nomenclatureRepository,
				userRepository);

		protected override Func<NomenclatureJournalNode, NomenclatureViewModel> OpenDialogFunction =>
			node => new NomenclatureViewModel(EntityUoWBuilder.ForOpen(node.Id), UnitOfWorkFactory, commonServices,
				employeeService, nomenclatureSelectorFactory, counterpartySelectorFactory, nomenclatureRepository,
				userRepository);
	}
}
