using NHibernate;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using Autofac;
using NHibernate.Criterion;
using QS.Navigation;
using Vodovoz.Domain.Permissions.Warehouses;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.ViewModels.Journals.FilterViewModels.Store;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Warehouses;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Store
{
	public class WarehouseJournalViewModel : SingleEntityJournalViewModelBase<Warehouse, WarehouseViewModel, WarehouseJournalNode>
	{
		private readonly ISubdivisionRepository _subdivisionRepository;
		private ILifetimeScope _lifetimeScope;
		private WarehouseJournalFilterViewModel _filterViewModel;
		private WarehousePermissionsType[] _warehousePermissions;
		
		public WarehouseJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			ISubdivisionRepository subdivisionRepository,
			INavigationManager navigationManager,
			ILifetimeScope lifetimeScope,
			Action<WarehouseJournalFilterViewModel> filterParams = null)
				: base(unitOfWorkFactory, commonServices, navigation: navigationManager)
		{
			TabName = "Журнал складов";
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
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
			_filterViewModel = filter;
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
			CreateDefaultEditAction();
			CreateDefaultAddActions();
		}

		protected override Func<IUnitOfWork, IQueryOver<Warehouse>> ItemsSourceQueryFunction => (uow) => {
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
			foreach (var p in _warehousePermissions)
			{
				disjunction.Add<Warehouse>(
					w =>
						w.Id.IsIn(permission.WarehousePermissions.Where(x => x.WarehousePermissionType == p && x.PermissionValue == true)
						.Select(x => x.Warehouse.Id).ToArray()));
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
		};

		protected override Func<WarehouseViewModel> CreateDialogFunction => () => new WarehouseViewModel(
			EntityUoWBuilder.ForCreate(),
			QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
			commonServices,
			_subdivisionRepository,
			NavigationManager,
			_lifetimeScope
		);

		protected override Func<WarehouseJournalNode, WarehouseViewModel> OpenDialogFunction => node => new WarehouseViewModel(
			EntityUoWBuilder.ForOpen(node.Id),
			QS.DomainModel.UoW.UnitOfWorkFactory.GetDefaultFactory,
			commonServices,
			_subdivisionRepository,
			NavigationManager,
			_lifetimeScope
		);

		public override void Dispose()
		{
			_lifetimeScope = null;
			base.Dispose();
		}
	}
}
