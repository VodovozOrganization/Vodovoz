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
using Vodovoz.ViewModels.Journals.JournalNodes.Payments;
using Vodovoz.ViewModels.ViewModels.Payments;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Payments
{
	public class NotAllocatedCounterpartiesJournalViewModel
		: EntityJournalViewModelBase<NotAllocatedCounterparty, NotAllocatedCounterpartyViewModel, NotAllocatedCounterpartiesJournalNode>
	{
		public NotAllocatedCounterpartiesJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			IDeleteEntityService deleteEntityService,
			ICurrentPermissionService currentPermissionService)
			: base(unitOfWorkFactory, interactiveService, navigationManager, deleteEntityService, currentPermissionService)
		{
			VisibleDeleteAction = false;
		}

		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();

			var canCreate = CurrentPermissionService.ValidateEntityPermission(typeof(NotAllocatedCounterparty)).CanCreate;
			var canRead = CurrentPermissionService.ValidateEntityPermission(typeof(NotAllocatedCounterparty)).CanRead;
			var canDelete = CurrentPermissionService.ValidateEntityPermission(typeof(NotAllocatedCounterparty)).CanDelete;

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
				(selected) => selected.Cast<NotAllocatedCounterpartiesJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
				RowActivatedAction = editAction;

			var deleteAction = new JournalAction("Удалить",
				(selected) => canDelete && selected.Any(),
				(selected) => VisibleDeleteAction,
				(selected) => DeleteEntities(selected.Cast<NotAllocatedCounterpartiesJournalNode>().ToArray()),
				"Delete"
			);
			
			NodeActionsList.Add(deleteAction);
		}

		protected override IQueryOver<NotAllocatedCounterparty> ItemsQuery(IUnitOfWork uow)
		{
			ProfitCategory profitCategoryAlias = null;
			NotAllocatedCounterparty notAllocatedCounterpartyAlias = null;
			NotAllocatedCounterpartiesJournalNode resultyAlias = null;
			
			var query = uow.Session.QueryOver(() => notAllocatedCounterpartyAlias)
				.JoinAlias(() => notAllocatedCounterpartyAlias.ProfitCategory, () => profitCategoryAlias);

			query.Where(GetSearchCriterion(
				() => notAllocatedCounterpartyAlias.Inn
				));
			
			query.SelectList(list => list
				.Select(() => notAllocatedCounterpartyAlias.Id).WithAlias(() => resultyAlias.Id)
				.Select(() => notAllocatedCounterpartyAlias.Inn).WithAlias(() => resultyAlias.Inn)
				.Select(() => notAllocatedCounterpartyAlias.Name).WithAlias(() => resultyAlias.Name)
				.Select(() => notAllocatedCounterpartyAlias.IsArchive).WithAlias(() => resultyAlias.IsArchive)
				.Select(() => profitCategoryAlias.Name).WithAlias(() => resultyAlias.ProfitCategory)
			)
			.TransformUsing(Transformers.AliasToBean<NotAllocatedCounterpartiesJournalNode>());
			
			return query;
		}
	}
}
