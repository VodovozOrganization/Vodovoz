using System;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using Vodovoz.Core.Domain.Payments;
using Vodovoz.Filters.ViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.ViewModels.Payments;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Payments
{
	public class ProfitCategoriesJournalViewModel : EntityJournalViewModelBase<ProfitCategory, ProfitCategoryViewModel, ProfitCategoriesJournalNode>
	{
		private readonly ProfitCategoriesJournalFilterViewModel _filterViewModel;

		public ProfitCategoriesJournalViewModel(
			ProfitCategoriesJournalFilterViewModel filterViewModel,
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService,
			Action<ProfitCategoriesJournalFilterViewModel> filterParams = null)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			_filterViewModel = filterViewModel ?? throw new ArgumentNullException(nameof(filterViewModel));
			JournalFilter = _filterViewModel;
			_filterViewModel.OnFiltered += OnFilterFiltered;

			if(filterParams != null)
			{
				_filterViewModel.ConfigureWithoutFiltering(filterParams);
			}

			VisibleDeleteAction = false;
		}
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();

			var canCreate = CurrentPermissionService.ValidateEntityPermission(typeof(ProfitCategory)).CanCreate;
			var canRead = CurrentPermissionService.ValidateEntityPermission(typeof(ProfitCategory)).CanUpdate;
			var canDelete = CurrentPermissionService.ValidateEntityPermission(typeof(ProfitCategory)).CanDelete;

			var addAction = new JournalAction("Добавить",
				(selected) => canCreate,
				(selected) => VisibleCreateAction,
				(selected) => CreateEntityDialog(),
				"Insert"
			);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
				(selected) => canRead && selected.Any(),
				(selected) => VisibleEditAction,
				(selected) => selected.Cast<ProfitCategoriesJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
				RowActivatedAction = editAction;

			var deleteAction = new JournalAction("Удалить",
				(selected) => canDelete && selected.Any(),
				(selected) => VisibleDeleteAction,
				(selected) => DeleteEntities(selected.Cast<ProfitCategoriesJournalNode>().ToArray()),
				"Delete"
			);
			NodeActionsList.Add(deleteAction);
		}

		protected override IQueryOver<ProfitCategory> ItemsQuery(IUnitOfWork uow)
		{
			ProfitCategory profitCategoryAlias = null;
			ProfitCategoriesJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => profitCategoryAlias);

			if(!_filterViewModel.ShowArchive)
			{
				query.Where(() => !profitCategoryAlias.IsArchive);
			}

			query.Where(GetSearchCriterion(
					() => profitCategoryAlias.Id,
					() => profitCategoryAlias.Name));

			query.SelectList(list => list
					.Select(() => profitCategoryAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => profitCategoryAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(() => profitCategoryAlias.IsArchive).WithAlias(() => resultAlias.IsArchive)
				)
				.TransformUsing(Transformers.AliasToBean<ProfitCategoriesJournalNode>());

			return query;
		}
		
		private void OnFilterFiltered(object sender, EventArgs e)
		{
			Refresh();
		}
	}
}
