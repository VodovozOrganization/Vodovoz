using System;
using System.Linq;
using NHibernate;
using NHibernate.Transform;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.ViewModels.Journals.JournalNodes.Orders;
using Vodovoz.ViewModels.ViewModels.Orders;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Orders
{
	public class RequestsForCallClosedReasonsJournalViewModel
		: EntityJournalViewModelBase<
			RequestForCallClosedReason,
			RequestForCallClosedReasonViewModel,
			RequestsForCallClosedReasonsJournalNode>
	{
		public RequestsForCallClosedReasonsJournalViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager,
			ICurrentPermissionService currentPermissionService
		) : base(unitOfWorkFactory, interactiveService, navigationManager, currentPermissionService: currentPermissionService)
		{
			if(unitOfWorkFactory is null)
			{
				throw new ArgumentNullException(nameof(unitOfWorkFactory));
			}
			if(navigationManager is null)
			{
				throw new ArgumentNullException(nameof(navigationManager));
			}

			TabName = "Журнал причин закрытия заявок на звонок";
			VisibleDeleteAction = false;
		}
		
		protected override void CreateNodeActions()
		{
			NodeActionsList.Clear();
			CreateDefaultSelectAction();

			bool canCreate = CurrentPermissionService.ValidateEntityPermission(typeof(RequestForCallClosedReason)).CanCreate;
			bool canEdit = CurrentPermissionService.ValidateEntityPermission(typeof(RequestForCallClosedReason)).CanRead;

			var addAction = new JournalAction("Добавить",
				(selected) => canCreate,
				(selected) => VisibleCreateAction,
				(selected) => CreateEntityDialog(),
				"Insert"
			);
			NodeActionsList.Add(addAction);

			var editAction = new JournalAction("Изменить",
				(selected) => canEdit && selected.Any(),
				(selected) => VisibleEditAction,
				(selected) => selected.Cast<RequestsForCallClosedReasonsJournalNode>().ToList().ForEach(EditEntityDialog)
			);
			NodeActionsList.Add(editAction);

			if(SelectionMode == JournalSelectionMode.None)
			{
				RowActivatedAction = editAction;
			}
		}

		protected override IQueryOver<RequestForCallClosedReason> ItemsQuery(IUnitOfWork uow)
		{
			RequestsForCallClosedReasonsJournalNode resultAlias = null;

			var query = uow.Session.QueryOver<RequestForCallClosedReason>()
				.SelectList(list => list
					.Select(r => r.Id).WithAlias(() => resultAlias.Id)
					.Select(r => r.IsArchive).WithAlias(() => resultAlias.IsArchive)
					.Select(r => r.Name).WithAlias(() => resultAlias.Name))
				.TransformUsing(Transformers.AliasToBean<RequestsForCallClosedReasonsJournalNode>());

			return query;
		}
	}
}
