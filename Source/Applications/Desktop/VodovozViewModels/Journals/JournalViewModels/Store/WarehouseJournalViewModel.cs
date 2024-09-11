using Autofac;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Store
{
	public class WarehouseJournalViewModel : EntityJournalViewModelBase<Warehouse, WarehouseViewModel, WarehouseJournalNode>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private WarehouseJournalFilterViewModel _filterViewModel;
		private WarehousePermissionsType[] _warehousePermissions;
		
		public WarehouseJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ISubdivisionRepository subdivisionRepository,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			Action<WarehouseJournalFilterViewModel> filterParams = null)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			TabName = "Журнал складов";
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_warehousePermissions = new[] { WarehousePermissionsType.WarehouseView };

			CreateFilter(filterParams);
			
			UpdateOnChanges(
				typeof(Warehouse)
			);
		}

		private void CreateFilter(Action<WarehouseJournalFilterViewModel> filterParams)
		{
			var filter = new WarehouseJournalFilterViewModel();
			filterParams?.Invoke(filter);
			JournalFilter = filter;
			_filterViewModel = filter;
		}

		protected override IQueryOver<Warehouse> ItemsQuery(IUnitOfWork uow)
		{
			Warehouse warehouseAlias = null;
			WarehouseJournalNode warehouseNodeAlias = null;

			var query = uow.Session.QueryOver<Warehouse>(() => warehouseAlias).WhereNot(w => w.IsArchive);
			var disjunction = new Disjunction();

			if(_filterViewModel?.ExcludeWarehousesIds != null)
			{
				query.WhereRestrictionOn(x => x.Id).Not.IsIn(_filterViewModel.ExcludeWarehousesIds);
			}

			if(_filterViewModel?.IncludeWarehouseIds != null)
			{
				query.WhereRestrictionOn(x => x.Id).IsInG(_filterViewModel.IncludeWarehouseIds);
			}

			var permission = new CurrentWarehousePermissions();

			foreach(var p in _warehousePermissions)
			{
				disjunction.Add<Warehouse>(
					w =>
						w.Id.IsIn(permission.WarehousePermissions
							.Where(x => x.WarehousePermissionType == p && x.PermissionValue == true)
							.Select(x => x.Warehouse.Id)
							.ToArray()));
			}

			query.Where(disjunction);

			query.Where(GetSearchCriterion(
				() => warehouseAlias.Id,
				() => warehouseAlias.Name
			));

			var result = query.SelectList(list => list
					.Select(w => w.Id).WithAlias(() => warehouseNodeAlias.Id)
					.Select(w => w.Name).WithAlias(() => warehouseNodeAlias.Name))
				.OrderBy(w => w.Name).Asc
				.TransformUsing(Transformers.AliasToBean<WarehouseJournalNode>());

			return result;
		}
	}
}
