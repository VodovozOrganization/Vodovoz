using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Project.Services;
using QS.Services;
using System;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Tools;
using Vodovoz.ViewModels.Employees;
using Vodovoz.ViewModels.Journals.Nodes.Employees;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Employees
{
	public class FineCategoryJournalViewModel : EntityJournalViewModelBase<
			FineCategory,
			FineCategoryViewModel,
			FineCategoryJournalNode>
	{
		private readonly bool _canWorkWithFineCategories;

		public FineCategoryJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService,
			IDeleteEntityService deleteEntityService = null) : base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			_canWorkWithFineCategories = currentPermissionService.ValidatePresetPermission(
				Core.Domain.Permissions.EmployeePermissions.CanWorkWithFineCategories);

			TabName = $"Журнал {typeof(FineCategory).GetClassUserFriendlyName().GenitivePlural}";
		}
		protected override void CreateNodeActions()
		{
			var addAction = new JournalAction("Добавить",
					(selected) => _canWorkWithFineCategories,
					(selected) => VisibleCreateAction,
					(selected) => CreateEntityDialog(),
					"Insert"
					);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
					(selected) => _canWorkWithFineCategories && selected.Any(),
					(selected) => VisibleEditAction,
					(selected) => selected.Cast<FineCategoryJournalNode>().ToList().ForEach(base.EditEntityDialog)
					);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
				RowActivatedAction = editAction;

			var deleteAction = new JournalAction("Удалить",
					(selected) => _canWorkWithFineCategories && selected.Any(),
					(selected) => VisibleDeleteAction,
					(selected) => DeleteEntities(selected.Cast<FineCategoryJournalNode>().ToArray()),
					"Delete"
					);
			NodeActionsList.Add(deleteAction);
		}

		protected override IQueryOver<FineCategory> ItemsQuery(IUnitOfWork uow)
		{
			FineCategory fineCategoryAlias = null;
			FineCategoryJournalNode resultAlias = null;

			var query = uow.Session.QueryOver(() => fineCategoryAlias);
			return query
				.SelectList(list => list
					.Select(() => fineCategoryAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => fineCategoryAlias.Name).WithAlias(() => resultAlias.FineCategoryName)
				)
				.TransformUsing(Transformers.AliasToBean<FineCategoryJournalNode>());
		}
	}
}
