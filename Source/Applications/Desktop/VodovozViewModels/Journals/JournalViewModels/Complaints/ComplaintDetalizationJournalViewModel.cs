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
using System;
using System.Linq;
using Vodovoz.Domain.Complaints;
using Vodovoz.ViewModels.Journals.FilterViewModels.Complaints;
using Vodovoz.ViewModels.Journals.JournalNodes.Complaints;
using Vodovoz.ViewModels.ViewModels.Complaints;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Complaints
{
	public class ComplaintDetalizationJournalViewModel
		: EntityJournalViewModelBase<
			ComplaintDetalization,
			ComplaintDetalizationViewModel,
			ComplaintDetalizationJournalNode>
	{
		private ComplaintDetalizationJournalFilterViewModel _filterViewModel;
		private IPermissionResult _premissionResult;
		private readonly ILifetimeScope _scope;

		public ComplaintDetalizationJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService,
			ILifetimeScope scope,
			Action<ComplaintDetalizationJournalFilterViewModel> filterParams = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService)
		{
			_scope = scope ?? throw new ArgumentNullException(nameof(scope));

			TabName = "Детализации видов рекламаций";

			CreateFilter(filterParams);

			UpdateOnChanges(
				typeof(ComplaintKind),
				typeof(ComplaintObject),
				typeof(ComplaintDetalization));
		}

		private void CreateFilter(Action<ComplaintDetalizationJournalFilterViewModel> filterParams)
		{
			_filterViewModel = (filterParams is null)
				? _scope.Resolve<ComplaintDetalizationJournalFilterViewModel>()
				: _scope.Resolve<ComplaintDetalizationJournalFilterViewModel>(
				new TypedParameter(typeof(Action<ComplaintDetalizationJournalFilterViewModel>),
				filterParams));
			_filterViewModel.OnFiltered += OnFilterViewModelFiltered;
			JournalFilter = _filterViewModel;
		}

		private void OnFilterViewModelFiltered(object sender, EventArgs e)
		{
			Refresh();
		}

		protected override IQueryOver<ComplaintDetalization> ItemsQuery(IUnitOfWork unitOfWork)
		{
			ComplaintDetalization complaintDetalizationAlias = null;
			ComplaintKind complaintKindAlias = null;
			ComplaintObject complaintObjectAlias = null;
			ComplaintDetalizationJournalNode resultAlias = null;

			var itemsQuery = unitOfWork.Session.QueryOver(() => complaintDetalizationAlias)
				.Left.JoinAlias(x => x.ComplaintKind, () => complaintKindAlias)
				.Left.JoinAlias(() => complaintKindAlias.ComplaintObject, () => complaintObjectAlias);

			if(_filterViewModel.ComplaintObject != null)
			{
				var complaintIbjectId = _filterViewModel.ComplaintObject.Id;
				itemsQuery.Where(() => complaintObjectAlias.Id == complaintIbjectId);
			}

			if(_filterViewModel.ComplaintKind != null)
			{
				var complaintKindId = _filterViewModel.ComplaintKind.Id;
				itemsQuery.Where(() => complaintKindAlias.Id == complaintKindId);
			}

			if(_filterViewModel.HideArchieve)
			{
				itemsQuery.Where(() => complaintDetalizationAlias.IsArchive == false);
			}

			itemsQuery.Where(GetSearchCriterion(
				() => complaintDetalizationAlias.Id,
				() => complaintDetalizationAlias.Name,
				() => complaintKindAlias.Name,
				() => complaintObjectAlias.Name));

			itemsQuery.OrderBy(x => x.IsArchive).Asc
				.ThenBy(x => x.Id).Asc
				.SelectList(list =>
					list.SelectGroup(() => complaintDetalizationAlias.Id).WithAlias(() => resultAlias.Id)
						.Select(() => complaintDetalizationAlias.Name).WithAlias(() => resultAlias.Name)
						.Select(() => complaintDetalizationAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
						.Select(() => complaintObjectAlias.Name).WithAlias(() => resultAlias.ComplaintObject)
						.Select(() => complaintKindAlias.Name).WithAlias(() => resultAlias.ComplaintKind))
				.TransformUsing(Transformers.AliasToBean<ComplaintDetalizationJournalNode>());

			return itemsQuery;
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
				(selected) => selected.OfType<ComplaintDetalizationJournalNode>().ToList().ForEach(EditEntityDialog)
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

		protected override void EditEntityDialog(ComplaintDetalizationJournalNode node)
		{
			NavigationManager.OpenViewModel<ComplaintDetalizationViewModel, IEntityUoWBuilder, ComplaintObject, ComplaintKind>(
				this, EntityUoWBuilder.ForOpen(DomainHelper.GetId(node)),
				_filterViewModel.RestrictComplaintObject,
				_filterViewModel.RestrictComplaintKind);
		}

		protected override void CreateEntityDialog()
		{
			NavigationManager.OpenViewModel<ComplaintDetalizationViewModel, IEntityUoWBuilder, ComplaintObject, ComplaintKind>(
				this,
				EntityUoWBuilder.ForCreate(),
				_filterViewModel.RestrictComplaintObject,
				_filterViewModel.RestrictComplaintKind);
		}

		public override void Dispose()
		{
			_filterViewModel.OnFiltered -= OnFilterViewModelFiltered;
			base.Dispose();
		}
	}
}
