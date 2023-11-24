using Autofac;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Sale;
using Vodovoz.ViewModels.Dialogs.Sales;
using Vodovoz.ViewModels.Journals.FilterViewModels.GeoGroup;
using Vodovoz.ViewModels.Journals.JournalNodes;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Sale
{
	public class GeoGroupJournalViewModel : EntityJournalViewModelBase<GeoGroup, GeoGroupViewModel, GeoGroupJournalNode>
	{
		private readonly ILifetimeScope _lifetimeScope;

		private GeoGroupJournalFilterViewModel _filterViewModel;
		private IPermissionResult _premissionResult;

		public GeoGroupJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService,
			ILifetimeScope lifetimeScope,
			Action<GeoGroupJournalFilterViewModel> filterParams = null
			) : base(unitOfWorkFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService)
		{
			_lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));

			CreateFilter(filterParams);

			Title = "Части города";
		}

		private void CreateFilter(Action<GeoGroupJournalFilterViewModel> filterParams)
		{
			_filterViewModel = (filterParams is null)
				? _lifetimeScope.Resolve<GeoGroupJournalFilterViewModel>()
				: _lifetimeScope.Resolve<GeoGroupJournalFilterViewModel>(
				new TypedParameter(typeof(Action<GeoGroupJournalFilterViewModel>),
				filterParams));
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		public void DisableChangeEntityActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();
		}

		protected override IQueryOver<GeoGroup> ItemsQuery(IUnitOfWork uow)
		{
			GeoGroup geoGroupAlias = null;
			GeoGroupJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => geoGroupAlias);

			if(_filterViewModel?.IsArchive != null)
			{
				query.Where(() => geoGroupAlias.IsArchived == _filterViewModel.IsArchive);
			}

			query.Where(GetSearchCriterion(
				() => geoGroupAlias.Name,
				() => geoGroupAlias.Id
			));

			var items = query
				.SelectList(list => list
				   .Select(() => geoGroupAlias.Id).WithAlias(() => resultAlias.Id)
				   .Select(() => geoGroupAlias.Name).WithAlias(() => resultAlias.Name))
				.TransformUsing(Transformers.AliasToBean<GeoGroupJournalNode>());

			return items;
		}

		protected override void CreateNodeActions()
		{
			_premissionResult = CurrentPermissionService.ValidateEntityPermission(typeof(ComplaintDetalization));

			NodeActionsList.Clear();

			CreateSelectAction();

			var addAction = new JournalAction("Добавить",
				(selected) => _premissionResult.CanCreate,
				(selected) => VisibleCreateAction,
				(selected) => CreateEntityDialog(),
				"Insert"
			);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
				(selected) => _premissionResult.CanUpdate && selected.Any(),
				(selected) => VisibleEditAction,
				(selected) => selected.OfType<GeoGroupJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

			var deleteAction = new JournalAction("Удалить",
					(selected) => _premissionResult.CanDelete && selected.Any(),
					(selected) => VisibleDeleteAction,
					(selected) => DeleteEntities(selected.OfType<GeoGroupJournalNode>().ToArray())
				);
			NodeActionsList.Add(deleteAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
		}

		private void CreateSelectAction()
		{
			var selectAction = new JournalAction("Выбрать",
				(selected) => selected.Any(),
				(selected) => SelectionMode != JournalSelectionMode.None,
				(selected) => OnItemsSelected(selected)
			);

			if(SelectionMode == JournalSelectionMode.Single || SelectionMode == JournalSelectionMode.Multiple)
			{
				RowActivatedAction = selectAction;
			}

			NodeActionsList.Add(selectAction);
		}

		//protected override Func<GeoGroupViewModel> CreateDialogFunction => () =>
		//	new GeoGroupViewModel(
		//		EntityUoWBuilder.ForCreate(),
		//		_navigationManager,
		//		UnitOfWorkFactory,
		//		_geoGroupVersionsModel,
		//		_warehouseJournalFactory,
		//		commonServices,
		//		_lifetimeScope);

		//protected override Func<GeoGroupJournalNode, GeoGroupViewModel> OpenDialogFunction => node =>
		//	new GeoGroupViewModel(
		//		EntityUoWBuilder.ForOpen(node.Id),
		//		_navigationManager,
		//		UnitOfWorkFactory,
		//		_geoGroupVersionsModel,
		//		_warehouseJournalFactory,
		//		commonServices,
		//		_lifetimeScope);
	}
}
