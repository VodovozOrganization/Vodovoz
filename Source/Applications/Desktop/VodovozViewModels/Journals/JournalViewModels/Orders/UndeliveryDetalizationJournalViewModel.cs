using System;
using System.Linq;
using Autofac;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.FilterViewModels.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class UndeliveryDetalizationJournalViewModel
		: EntityJournalViewModelBase<
			UndeliveryDetalization,
			UndeliveryDetalizationViewModel,
			UndeliveryDetalizationJournalNode>
	{
		private UndeliveryDetalizationJournalFilterViewModel _filterViewModel;
		private IPermissionResult _premissionResult;
		private readonly ILifetimeScope _scope;

		public UndeliveryDetalizationJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService,
			ILifetimeScope scope,
			Action<UndeliveryDetalizationJournalFilterViewModel> filterParams = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			TabName = "Детализации видов недовоза";

			CreateFilter(filterParams);

			UpdateOnChanges(
				typeof(UndeliveryKind),
				typeof(UndeliveryObject),
				typeof(UndeliveryDetalization));
		}

		private void CreateFilter(Action<UndeliveryDetalizationJournalFilterViewModel> filterParams)
		{
			_filterViewModel = (filterParams is null)
				? _scope.Resolve<UndeliveryDetalizationJournalFilterViewModel>()
				: _scope.Resolve<UndeliveryDetalizationJournalFilterViewModel>(
				new TypedParameter(typeof(Action<UndeliveryDetalizationJournalFilterViewModel>),
				filterParams));
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override IQueryOver<UndeliveryDetalization> ItemsQuery(IUnitOfWork unitOfWork)
		{
			UndeliveryDetalization undeliveryDetalizationAlias = null;
			UndeliveryKind undeliveryKindAlias = null;
			UndeliveryObject undeliveryObjectAlias = null;
			UndeliveryDetalizationJournalNode resultAlias = null;

			var itemsQuery = unitOfWork.Session.QueryOver(() => undeliveryDetalizationAlias)
				.Left.JoinAlias(x => x.UndeliveryKind, () => undeliveryKindAlias)
				.Left.JoinAlias(() => undeliveryKindAlias.UndeliveryObject, () => undeliveryObjectAlias);

			if(_filterViewModel.UndeliveryObject != null)
			{
				var undeliveryIbjectId = _filterViewModel.UndeliveryObject.Id;
				itemsQuery.Where(() => undeliveryObjectAlias.Id == undeliveryIbjectId);
			}

			if(_filterViewModel.UndeliveryKind != null)
			{
				var undeliveryKindId = _filterViewModel.UndeliveryKind.Id;
				itemsQuery.Where(() => undeliveryKindAlias.Id == undeliveryKindId);
			}

			if(_filterViewModel.HideArchieve)
			{
				itemsQuery.Where(() => undeliveryDetalizationAlias.IsArchive == false);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => undeliveryDetalizationAlias.Id,
				() => undeliveryDetalizationAlias.Name,
				() => undeliveryKindAlias.Name,
				() => undeliveryObjectAlias.Name));

			itemsQuery.OrderBy(x => x.IsArchive).Asc
				.ThenBy(x => x.Id).Asc
				.SelectList(list =>
					list.SelectGroup(() => undeliveryDetalizationAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => undeliveryDetalizationAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => undeliveryDetalizationAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
						.Select(() => undeliveryObjectAlias.Name).WithAlias(() => resultAlias.UndeliveryObject)
						.Select(() => undeliveryKindAlias.Name).WithAlias(() => resultAlias.UndeliveryKind))
				.TransformUsing(Transformers.AliasToBean<UndeliveryDetalizationJournalNode>());

			return itemsQuery;
		}

		protected override void CreateNodeActions()
		{
			_premissionResult = CurrentPermissionService.ValidateEntityPermission(typeof(UndeliveryDetalization));

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
				(selected) => selected.OfType<UndeliveryDetalizationJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

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

		protected override void EditEntityDialog(UndeliveryDetalizationJournalNode node)
		{
			NavigationManager.OpenViewModel<UndeliveryDetalizationViewModel, IEntityUoWBuilder, UndeliveryObject, UndeliveryKind>(
				this, EntityUoWBuilder.ForOpen(DomainHelper.GetId(node)),
				_filterViewModel.RestrictUndeliveryObject,
				_filterViewModel.RestrictUndeliveryKind);
		}

		protected override void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<UndeliveryDetalizationViewModel, IEntityUoWBuilder, UndeliveryObject, UndeliveryKind>(
				this,
				EntityUoWBuilder.ForCreate(),
				_filterViewModel.RestrictUndeliveryObject,
				_filterViewModel.RestrictUndeliveryKind);
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}
